using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Models;
using Vortex.Protocol.Packets;
using TShockAPI;

namespace Vortex.Adapter.Processing;

public class ServerOnlineHandler(Net.VortexClient client) : RequestHandlerBase<ServerOnlinePacket, ServerOnlinePacketResponse>(client)
{
    public override ServerOnlinePacketResponse Handle(ServerOnlinePacket request)
    {
        var players = TShock.Players
            .Where(x => x != null && x.Active)
            .Select(x => new Player
            {
                Name = x.Name,
                Group = x.Group?.Name ?? string.Empty,
                Prefix = x.Group?.Prefix ?? string.Empty,
                IsLogin = x.IsLoggedIn
            })
            .ToList();

        return new ServerOnlinePacketResponse
        {
            RequestId = request.RequestId,
            Success = true,
            Message = "查询成功",
            Players = players,
            MaxCount = TShock.Config.Settings.MaxSlots,
            OnlineCount = players.Count
        };
    }
}
