using Vortex.Adapter.Net;
using Vortex.Adapter.Processing;
using Vortex.Protocol.Interfaces;

namespace Vortex.Adapter;

/// <summary>
/// 数据包处理器
/// 处理来自 Vortex 服务器的请求
/// </summary>
public class PacketHandler
{
    private readonly VortexClient _client;
    private readonly PacketHandlerManager _handlerManager;

    public PacketHandler(VortexClient client)
    {
        _client = client;
        _handlerManager = new PacketHandlerManager(client);
        client.OnPacketReceived += HandlePacket;
    }

    private void HandlePacket(INetPacket packet)
    {
        _ = Task.Run(async () =>
        {
            await _handlerManager.HandlePacketAsync(packet);
        });
    }
}
