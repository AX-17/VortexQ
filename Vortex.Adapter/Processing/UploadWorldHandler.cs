using Vortex.Protocol.Packets;
using Terraria;
using Terraria.IO;

namespace Vortex.Adapter.Processing;

public class UploadWorldHandler(Net.VortexClient client) : RequestHandlerBase<WorldFilePacket, WorldFilePacketResponse>(client)
{
    public override WorldFilePacketResponse Handle(WorldFilePacket request)
    {
        WorldFile.SaveWorld();
        var buffer = File.ReadAllBytes(Main.worldPathName);
        return new WorldFilePacketResponse
        {
            RequestId = request.RequestId,
            Success = true,
            Message = "上传成功",
            WorldName = Main.worldName,
            WorldBuffer = buffer
        };
    }
}
