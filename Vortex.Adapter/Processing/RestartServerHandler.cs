using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;

namespace Vortex.Adapter.Processing;

public class RestartServerHandler(Net.VortexClient client) : RequestHandlerBase<ServerRestartPacket, ServerRestartPacketResponse>(client)
{
    public override ServerRestartPacketResponse Handle(ServerRestartPacket request)
    {
        Utils.ReStarServer(request.StartArgs, true);
        return CreateSuccessResponse(request, "正在重启服务器");
    }
}
