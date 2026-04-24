using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;

namespace Vortex.Adapter.Processing;

public class OnlineRankHandler(Net.VortexClient client) : RequestHandlerBase<OnlineRankPacket, OnlineRankPacketResponse>(client)
{
    public override OnlineRankPacketResponse Handle(OnlineRankPacket request)
    {
        return new OnlineRankPacketResponse
        {
            RequestId = request.RequestId,
            Success = true,
            Message = "在线排行查询成功",
            OnlineRank = Plugin.Onlines.GetAll()
        };
    }
}
