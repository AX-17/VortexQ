using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;

namespace Vortex.Adapter.Processing;

public class GameProgressHandler(Net.VortexClient client) : RequestHandlerBase<GameProgressPacket, GameProgressPacketResponse>(client)
{
    public override GameProgressPacketResponse Handle(GameProgressPacket request)
    {
        return new GameProgressPacketResponse
        {
            RequestId = request.RequestId,
            Success = true,
            Message = "进度查询成功",
            Progress = Utils.GetGameProgress()
        };
    }
}
