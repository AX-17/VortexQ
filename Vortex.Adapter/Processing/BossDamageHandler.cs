using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;

namespace Vortex.Adapter.Processing;

public class BossDamageHandler(Net.VortexClient client) : RequestHandlerBase<BossDamagePacket, BossDamagePacketResponse>(client)
{
    public override BossDamagePacketResponse Handle(BossDamagePacket request)
    {
        return new BossDamagePacketResponse
        {
            RequestId = request.RequestId,
            Success = true,
            Message = "查询成功",
            Damages = [.. Plugin.DamageBoss.Values]
        };
    }
}
