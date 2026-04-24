using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;

namespace Vortex.Adapter.Processing;

public class DeathRankHandler(Net.VortexClient client) : RequestHandlerBase<DeathRankPacket, DeathRankPacketResponse>(client)
{
    public override DeathRankPacketResponse Handle(DeathRankPacket request)
    {
        return new DeathRankPacketResponse
        {
            RequestId = request.RequestId,
            Success = true,
            Message = "死亡排行查询成功",
            Rank = Plugin.Deaths.GetAll()
        };
    }
}
