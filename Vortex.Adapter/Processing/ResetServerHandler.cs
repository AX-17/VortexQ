using Vortex.Adapter.Setting.Configs;
using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;

namespace Vortex.Adapter.Processing;

public class ResetServerHandler(Net.VortexClient client) : RequestHandlerBase<ServerResetPacket, ServerResetPacketResponse>(client)
{
    public override ServerResetPacketResponse Handle(ServerResetPacket request)
    {
        Utils.RestServer(new ResetConfig
        {
            Commands = request.ResetCommand,
            StartArgs = request.StartArgs,
            UseFile = request.UseFile,
            FileName = request.FileName,
            FileBuffer = request.FileBuffer
        });
        return CreateSuccessResponse(request, "正在重置服务器");
    }
}
