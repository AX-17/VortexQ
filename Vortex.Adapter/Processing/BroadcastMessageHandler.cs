using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;
using TShockAPI;

namespace Vortex.Adapter.Processing;

public class BroadcastMessageHandler(Net.VortexClient client) : RequestHandlerBase<BroadcastMessagePacket, BroadcastMessagePacketResponse>(client)
{
    public override BroadcastMessagePacketResponse Handle(BroadcastMessagePacket request)
    {
        TShock.Utils.Broadcast(request.Text, request.Color[0], request.Color[1], request.Color[2]);
        return CreateSuccessResponse(request, "广播成功");
    }
}
