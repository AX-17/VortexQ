using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;
using TShockAPI;

namespace Vortex.Adapter.Processing;

public class PrivateMessageHandler(Net.VortexClient client) : RequestHandlerBase<PrivateMessagePacket, PrivateMessagePacketResponse>(client)
{
    public override PrivateMessagePacketResponse Handle(PrivateMessagePacket request)
    {
        var player = TShock.Players.FirstOrDefault(x => x != null && x.Name == request.Name && x.Active);
        player?.SendMessage(request.Text, request.Color[0], request.Color[1], request.Color[2]);
        return CreateSuccessResponse(request, "发送成功");
    }
}
