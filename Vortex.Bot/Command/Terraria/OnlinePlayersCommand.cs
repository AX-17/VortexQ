using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Vortex.Bot.Attributes;
using Vortex.Bot.Core.Service;

namespace Vortex.Bot.Command.Terraria;

[Command("在线", "online", "players")]
[HelpText("查看在线玩家")]
[CommandType(CommandType.Group)]
[Permission("vortex.terraria.online")]
public static class OnlinePlayersCommand
{
    [Main]
    public static async Task ShowOnlinePlayers(GroupCommandArgs args)
    {
        TerrariaServerService? serverManager = args.Context.Server?.Services.GetService<TerrariaServerService>();
        if (serverManager == null)
        {
            await args.ReplyWithAtAsync("服务器管理器未初始化");
            return;
        }

        long groupId = args.GroupUin;
        List<TerrariaServer> servers = groupId > 0
            ? [.. serverManager.GetServersByGroup(groupId)]
            : serverManager.GetAllServers().ToList();

        if (servers.Count == 0)
        {
            await args.ReplyWithAtAsync("此群未配置任何服务器!");
            return;
        }

        var sb = new StringBuilder();
        foreach (TerrariaServer? server in servers)
        {
            var online = await server.GetOnlinePlayersAsync();
            int playerCount = online?.Players?.Count ?? 0;
            int maxCount = online?.MaxCount ?? 0;

            sb.AppendLine($"[{server.Config.Name}] 在线玩家 ({playerCount}/{maxCount})");

            if (online?.Success == true && online.Players != null && online.Players.Count > 0)
            {
                sb.AppendLine(string.Join(", ", online.Players.Select(p => p.Name)));
            }
            else if (online?.Success == false)
            {
                sb.AppendLine("无法连接服务器");
            }
            else
            {
                sb.AppendLine("暂无在线玩家");
            }
            sb.AppendLine();
        }
        await args.ReplyAsync(sb.ToString().Trim());
    }
}
