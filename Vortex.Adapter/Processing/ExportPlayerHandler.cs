using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Models;
using Vortex.Protocol.Packets;
using TShockAPI;
using TShockAPI.DB;

namespace Vortex.Adapter.Processing;

public class ExportPlayerHandler(Net.VortexClient client) : RequestHandlerBase<PlayerExportPacket, PlayerExportPacketResponse>(client)
{
    public override PlayerExportPacketResponse Handle(PlayerExportPacket request)
    {
        var names = request.Names;
        if (names == null || names.Count == 0)
        {
            names = [.. TShock.UserAccounts.GetUserAccounts().Select(x => x.Name)];
        }

        var playerFiles = new List<PlayerArchive>();
        foreach (var name in names)
        {
            var archive = new PlayerArchive { Name = name };
            var account = TShock.UserAccounts.GetUserAccountByName(name);
            if (account == null)
            {
                archive.Active = false;
            }
            else
            {
                var playerData = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), account.ID);
                var player = Utils.CreateAPlayer(name, playerData);
                try
                {
                    archive.Buffer = Utils.ExportPlayer(player);
                    archive.Active = true;
                }
                catch
                {
                    continue;
                }
            }
            playerFiles.Add(archive);
        }

        return new PlayerExportPacketResponse
        {
            RequestId = request.RequestId,
            Success = true,
            Message = "导出成功",
            PlayerFiles = playerFiles
        };
    }
}
