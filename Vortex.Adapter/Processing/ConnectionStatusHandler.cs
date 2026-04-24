using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;

namespace Vortex.Adapter.Processing;

public class ConnectionStatusHandler(Net.VortexClient client) : RequestHandlerBase<ConnectionStatusPacket, ConnectionStatusPacketResponse>(client)
{
    public override ConnectionStatusPacketResponse Handle(ConnectionStatusPacket request)
    {
        switch (request.Status)
        {
            case SocketConnectType.Success:
                TShockAPI.TShock.Log.ConsoleInfo($"[Vortex.Adapter] 与服务器验证成功，已连接到 Vortex...");
                break;
            case SocketConnectType.VerifyError:
                TShockAPI.TShock.Log.ConsoleError($"[Vortex.Adapter] 通信令牌验证失败...");
                break;
            case SocketConnectType.ServerNull:
                TShockAPI.TShock.Log.ConsoleError($"[Vortex.Adapter] 无法在 Vortex 上找到此服务器...");
                break;
            default:
                TShockAPI.TShock.Log.ConsoleError("[Vortex.Adapter] 因未知错误无法验证通信令牌...");
                break;
        }

        return CreateSuccessResponse(request, "状态已接收");
    }
}
