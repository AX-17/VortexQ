using System.Net.Sockets;
using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;
using Vortex.Protocol.Serialization;

namespace Vortex.TestClient;

public class TestClient : IDisposable
{
    private readonly string _serverAddress;
    private readonly int _serverPort;
    private readonly string _clientName;
    private readonly PacketSerializer _serializer = new();

    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private Guid _clientId = Guid.NewGuid();
    private int _sessionId;
    private bool _isConnected;
    private bool _isAuthenticated;

    public event Action<string>? OnLog;
    public event Action<INetPacket>? OnPacketReceived;
    public event Action? OnConnected;
    public event Action? OnDisconnected;

    public bool IsConnected => _isConnected;
    public bool IsAuthenticated => _isAuthenticated;
    public Guid ClientId => _clientId;
    public int SessionId => _sessionId;

    public TestClient(string serverAddress, int serverPort, string clientName)
    {
        _serverAddress = serverAddress;
        _serverPort = serverPort;
        _clientName = clientName;
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_serverAddress, _serverPort);
            _stream = _tcpClient.GetStream();
            _isConnected = true;

            Log($"Connected to server {_serverAddress}:{_serverPort}");

            // Start receiving loop
            _ = ReceiveLoopAsync();

            OnConnected?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            Log($"Failed to connect: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AuthenticateAsync()
    {
        if (!_isConnected || _stream == null)
        {
            Log("Not connected to server");
            return false;
        }

        var identityPacket = new ClientIdentityPacket
        {
            ClientId = _clientId,
            ClientName = _clientName
        };

        await SendPacketAsync(identityPacket);
        Log($"Sent identity packet: {_clientName} ({_clientId})");

        // Wait for response
        var tcs = new TaskCompletionSource<bool>();
        Action<INetPacket>? handler = null;
        handler = packet =>
        {
            if (packet is ClientIdentityResponsePacket response)
            {
                if (response.Success)
                {
                    _sessionId = response.SessionId;
                    _isAuthenticated = true;
                    Log($"Authenticated successfully. Session ID: {_sessionId}");
                    tcs.TrySetResult(true);
                }
                else
                {
                    Log($"Authentication failed: {response.Message}");
                    tcs.TrySetResult(false);
                }
                OnPacketReceived -= handler;
            }
        };

        OnPacketReceived += handler;

        using var cts = new CancellationTokenSource(5000);
        await using (cts.Token.Register(() => tcs.TrySetCanceled()))
        {
            try
            {
                return await tcs.Task;
            }
            catch (OperationCanceledException)
            {
                Log("Authentication timeout");
                OnPacketReceived -= handler;
                return false;
            }
        }
    }

    public async Task SendPacketAsync(INetPacket packet)
    {
        if (_stream == null || !_isConnected)
        {
            Log("Cannot send packet: not connected");
            return;
        }

        var buffer = _serializer.Serialize(packet);
        await _stream.WriteAsync(buffer);
        Log($"Sent packet: {packet.PacketID}");
    }

    public async Task<TResponse?> RequestAsync<TRequest, TResponse>(TRequest request, int timeoutMs = 5000)
        where TRequest : IServicePacket
        where TResponse : class, IClientPacket
    {
        if (!_isAuthenticated)
        {
            Log("Cannot send request: not authenticated");
            return null;
        }

        var tcs = new TaskCompletionSource<IClientPacket>();
        Action<INetPacket>? handler = null;
        handler = packet =>
        {
            if (packet is TResponse response && response.RequestId == request.RequestId)
            {
                tcs.TrySetResult(response);
                OnPacketReceived -= handler;
            }
        };

        OnPacketReceived += handler;

        await SendPacketAsync(request);
        Log($"Sent request: {request.PacketID}, waiting for response...");

        using var cts = new CancellationTokenSource(timeoutMs);
        await using (cts.Token.Register(() => tcs.TrySetCanceled()))
        {
            try
            {
                var result = await tcs.Task;
                return result as TResponse;
            }
            catch (OperationCanceledException)
            {
                Log("Request timeout");
                OnPacketReceived -= handler;
                return null;
            }
        }
    }

    private async Task ReceiveLoopAsync()
    {
        if (_stream == null) return;

        try
        {
            while (_isConnected)
            {
                var lengthBytes = new byte[2];
                int read = await _stream.ReadAsync(lengthBytes.AsMemory(0, 2));
                if (read < 2) break;

                var length = BitConverter.ToInt16(lengthBytes);

                var data = new byte[length];
                data[0] = lengthBytes[0];
                data[1] = lengthBytes[1];

                int totalRead = 2;
                while (totalRead < length)
                {
                    read = await _stream.ReadAsync(data.AsMemory(totalRead, length - totalRead));
                    if (read == 0) break;
                    totalRead += read;
                }

                if (totalRead < length) break;

                using var ms = new MemoryStream(data);
                using var br = new BinaryReader(ms);
                var packet = _serializer.Deserialize(br);

                if (packet != null)
                {
                    Log($"Received packet: {packet.PacketID}");
                    OnPacketReceived?.Invoke(packet);
                }
            }
        }
        catch (Exception ex)
        {
            Log($"Receive loop error: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    public void Disconnect()
    {
        _isConnected = false;
        _isAuthenticated = false;
        _stream?.Close();
        _tcpClient?.Close();
        Log("Disconnected from server");
        OnDisconnected?.Invoke();
    }

    private void Log(string message)
    {
        var logMessage = $"[{DateTime.Now:HH:mm:ss}] [{_clientName}] {message}";
        OnLog?.Invoke(logMessage);
        Console.WriteLine(logMessage);
    }

    public void Dispose()
    {
        Disconnect();
    }
}
