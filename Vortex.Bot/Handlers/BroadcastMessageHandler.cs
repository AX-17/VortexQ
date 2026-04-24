using Lagrange.Core.Message;
using Vortex.Bot.Models;
using Vortex.Bot.Processing;
using Vortex.Protocol.Packets;

namespace Vortex.Bot.Handlers;

public class BroadcastMessageHandler : RoutedRequestHandlerBase<BroadcastMessagePacket, BroadcastMessagePacketResponse>
{
    public override BroadcastMessagePacketResponse Handle(BroadcastMessagePacket request, PacketRouteContext context)
    {

        return CreateSuccessResponse(request, $"Message received from ");
    }
}
