using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;

namespace Vortex.Adapter.Processing;

public class QueryInventoryHandler(Net.VortexClient client) : RequestHandlerBase<PlayerInventoryPacket, PlayerInventoryPacketResponse>(client)
{
    public override PlayerInventoryPacketResponse Handle(PlayerInventoryPacket request)
    {
        var inventory = Utils.BInvSee(request.Name);
        return new PlayerInventoryPacketResponse
        {
            RequestId = request.RequestId,
            Success = inventory != null,
            Message = inventory != null ? "查询成功" : "玩家不存在",
            PlayerData = inventory
        };
    }
}
