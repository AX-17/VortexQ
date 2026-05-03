using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;

namespace Vortex.Adapter.Processing;

public class MapImageHandler(Net.VortexClient client) : RequestHandlerBase<MapImagePacket, MapImagePacketResponse>(client)
{
    public override MapImagePacketResponse Handle(MapImagePacket request)
    {
        var buffer = Utils.CreateMapBytes(request.ImageType);
        return new MapImagePacketResponse
        {
            RequestId = request.RequestId,
            Success = true,
            Message = "地图生成成功",
            Buffer = buffer
        };
    }
}
