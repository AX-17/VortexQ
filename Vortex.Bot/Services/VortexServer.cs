using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vortex.Bot.Configuration;
using Vortex.Bot.Models;
using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;
using Vortex.Protocol.Serialization;

namespace Vortex.Bot.Services;

/// <summary>
/// Vortex TCP 服务器
/// 负责监听客户端连接、接收包并分发处理
/// </summary>
public class VortexServer(
    ILogger<VortexServer> logger,
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    VortexContext vortexContext,
    ClientConnectionManager connectionManager,
    PacketHandlerManager handlerManager) : BackgroundService
{
    private readonly ILogger<VortexServer> _logger = logger;
    private readonly SocketConfiguration _config = configuration.GetSection("Core:Socket").Get<SocketConfiguration>() ?? new SocketConfiguration();
    private readonly PacketSerializer _serializer = new();
    private readonly VortexContext _vortexContext = vortexContext;

    // 子系统
    private readonly ClientConnectionManager _connectionManager = connectionManager;
    private readonly PacketHandlerManager _handlerManager = handlerManager;

    private TcpListener? _listener;
    private bool _isRunning;

    public VortexContext VortexContext => _vortexContext;
    public ClientConnectionManager Connections => _connectionManager;
    public PacketHandlerManager Handlers => _handlerManager;
    public bool IsRunning => _isRunning;
    public IServiceProvider Services => serviceProvider;

    public event Action<ClientConnection>? OnClientConnected
    {
        add => _connectionManager.OnClientConnected += value;
        remove => _connectionManager.OnClientConnected -= value;
    }

    public event Action<ClientConnection>? OnClientDisconnected
    {
        add => _connectionManager.OnClientDisconnected += value;
        remove => _connectionManager.OnClientDisconnected -= value;
    }

    public event Action<ClientConnection, INetPacket>? OnPacketReceived;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[VortexServer] Server is disabled in configuration");
            return;
        }

        _listener = new TcpListener(IPAddress.Any, _config.Port);
        _listener.Start();
        _isRunning = true;

        _logger.LogInformation("[VortexServer] Started on port {Port}", _config.Port);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                _ = HandleClientAsync(client, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[VortexServer] Stopping...");
        }
        finally
        {
            _isRunning = false;
            _listener?.Stop();
            await _connectionManager.DisconnectAllAsync();
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        var endpoint = tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        _logger.LogInformation("[VortexServer] Client {Endpoint} connected", endpoint);

        using var stream = tcpClient.GetStream();
        ClientConnection? client = null;

        try
        {
            // 第一步：等待认证
            var authPacket = await ReadPacketAsync(stream, cancellationToken);
            if (authPacket is not ClientAuthPacket auth)
            {
                _logger.LogWarning("[VortexServer] Client {Endpoint} did not send auth packet first", endpoint);
                await SendResponseAsync(stream, new ClientAuthResponsePacket { Success = false, Message = "Auth required first" }, cancellationToken);
                return;
            }

            // 验证 Token
            if (auth.Token != _config.Token)
            {
                _logger.LogWarning("[VortexServer] Client {Endpoint} auth failed: invalid token", endpoint);
                await SendResponseAsync(stream, new ClientAuthResponsePacket { Success = false, Message = "Invalid token" }, cancellationToken);
                return;
            }

            // 认证成功，发送响应
            await SendResponseAsync(stream, new ClientAuthResponsePacket { Success = true, Message = "Auth success" }, cancellationToken);
            _logger.LogInformation("[VortexServer] Client {Endpoint} authenticated successfully", endpoint);

            // 第二步：等待身份包
            var identityPacket = await ReadPacketAsync(stream, cancellationToken);
            if (identityPacket is not ClientIdentityPacket identity)
            {
                _logger.LogWarning("[VortexServer] Client {Endpoint} did not send identity packet", endpoint);
                return;
            }

            // 注册客户端
            client = HandleIdentity(identity, tcpClient, endpoint);
            await SendResponseAsync(stream, CreateIdentityResponse(client), cancellationToken);

            // 第三步：处理后续包
            while (!cancellationToken.IsCancellationRequested)
            {
                var packet = await ReadPacketAsync(stream, cancellationToken);
                if (packet == null) break;

                await ProcessAndRespondAsync(stream, packet, client, cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "[VortexServer] Client {ClientId} error: {Message}", client?.ClientId, ex.Message);
        }
        finally
        {
            if (client != null)
            {
                // 从 TerrariaServerManager 注销
                try
                {
                    var serverManager = Services.GetService(typeof(TerrariaServerManager)) as TerrariaServerManager;
                    serverManager?.UnregisterClientConnection(client.ClientId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[VortexServer] Failed to unregister client from TerrariaServerManager");
                }

                await _connectionManager.RemoveClientAsync(client.ClientId);
            }
            tcpClient.Close();
        }
    }

    private async Task<INetPacket?> ReadPacketAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var lengthBytes = new byte[2];
        int read = await stream.ReadAsync(lengthBytes.AsMemory(0, 2), cancellationToken);
        if (read < 2) return null;

        var length = BitConverter.ToInt16(lengthBytes);
        var data = new byte[length];
        data[0] = lengthBytes[0];
        data[1] = lengthBytes[1];

        int totalRead = 2;
        while (totalRead < length)
        {
            read = await stream.ReadAsync(data.AsMemory(totalRead, length - totalRead), cancellationToken);
            if (read == 0) break;
            totalRead += read;
        }

        if (totalRead < length)
        {
            _logger.LogError("[VortexServer] 数据读取不完整: {TotalRead}/{Length}", totalRead, length);
            return null;
        }

        _logger.LogDebug("[VortexServer] 收到数据: {Data}", BitConverter.ToString(data));

        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        try
        {
            return _serializer.Deserialize(br);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VortexServer] 反序列化失败");
            throw;
        }
    }

    private ClientConnection HandleIdentity(ClientIdentityPacket packet, TcpClient tcpClient, string endpoint)
    {
        var client = _connectionManager.RegisterClient(packet, tcpClient, endpoint);
        _logger.LogInformation(
            "[VortexServer] Client registered: {ClientName} ({ClientId})",
            client.ClientName, client.ClientId);

        // 注册到 TerrariaServerManager
        try
        {
            var serverManager = Services.GetService(typeof(TerrariaServerManager)) as TerrariaServerManager;
            serverManager?.RegisterClientConnection(client.ClientName, client.ClientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VortexServer] Failed to register client to TerrariaServerManager");
        }

        return client;
    }

    private static ClientIdentityResponsePacket CreateIdentityResponse(ClientConnection client)
    {
        return new ClientIdentityResponsePacket
        {
            RequestId = Guid.NewGuid(),
            Success = true,
            Message = "Registered successfully",
            ClientId = client.ClientId,
            SessionId = client.SessionId
        };
    }

    private async Task ProcessAndRespondAsync(
        NetworkStream stream,
        INetPacket packet,
        ClientConnection client,
        CancellationToken cancellationToken)
    {
        var context = new PacketRouteContext
        {
            SenderClientId = client.ClientId,
            SenderSessionId = client.SessionId,
            SenderConnection = client
        };

        _logger.LogInformation(
            "[VortexServer] Received {PacketID} from {ClientName}",
            packet.PacketID, client.ClientName);

        OnPacketReceived?.Invoke(client, packet);

        var response = await _handlerManager.ProcessAsync(packet, context);

        if (response != null)
        {
            await SendResponseAsync(stream, response, cancellationToken);
            _logger.LogInformation(
                "[VortexServer] Sent {PacketID} to {ClientName}",
                response.PacketID, client.ClientName);
        }
    }

    private async Task SendResponseAsync(NetworkStream stream, INetPacket packet, CancellationToken cancellationToken)
    {
        var buffer = _serializer.Serialize(packet);
        await stream.WriteAsync(buffer, cancellationToken);
    }

    #region 便捷方法

    /// <summary>
    /// 向指定客户端发送包
    /// </summary>
    public async Task<bool> SendToClientAsync(Guid clientId, INetPacket packet)
    {
        var client = _connectionManager.GetClient(clientId);
        if (client == null)
        {
            _logger.LogWarning("[VortexServer] Client {ClientId} not found", clientId);
            return false;
        }

        try
        {
            var buffer = _serializer.Serialize(packet);
            await client.Stream.WriteAsync(buffer);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VortexServer] Failed to send to {ClientId}", clientId);
            return false;
        }
    }

    /// <summary>
    /// 向指定会话发送包
    /// </summary>
    public Task<bool> SendToSessionAsync(int sessionId, INetPacket packet)
    {
        var client = _connectionManager.GetClientBySession(sessionId);
        return client != null ? SendToClientAsync(client.ClientId, packet) : Task.FromResult(false);
    }

    /// <summary>
    /// 广播给所有客户端
    /// </summary>
    public async Task<int> BroadcastAsync(INetPacket packet)
    {
        var clients = _connectionManager.GetAllClients();
        var tasks = clients.Select(c => SendToClientAsync(c.ClientId, packet)).ToArray();
        var results = await Task.WhenAll(tasks);
        return results.Count(r => r);
    }

    /// <summary>
    /// 向指定客户端发送请求并等待响应
    /// </summary>
    public async Task<TResponse?> RequestAsync<TRequest, TResponse>(Guid clientId, TRequest request, int timeoutMs = 5000)
        where TRequest : IServicePacket
        where TResponse : class, IClientPacket
    {
        if (!_connectionManager.IsOnline(clientId))
        {
            _logger.LogWarning("[VortexServer] Client {ClientId} is offline", clientId);
            return null;
        }

        var tcs = new TaskCompletionSource<IClientPacket>();
        Action<ClientConnection, INetPacket>? handler = null;

        handler = (conn, packet) =>
        {
            if (conn.ClientId == clientId && packet is TResponse response && response.RequestId == request.RequestId)
            {
                tcs.TrySetResult(response);
                OnPacketReceived -= handler;
            }
        };

        OnPacketReceived += handler;

        try
        {
            if (!await SendToClientAsync(clientId, request))
            {
                OnPacketReceived -= handler;
                return null;
            }

            using var cts = new CancellationTokenSource(timeoutMs);
            await using (cts.Token.Register(() => tcs.TrySetCanceled()))
            {
                var result = await tcs.Task;
                return result as TResponse;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[VortexServer] Request timeout for {ClientId}", clientId);
            OnPacketReceived -= handler;
            return null;
        }
    }

    #endregion
}
