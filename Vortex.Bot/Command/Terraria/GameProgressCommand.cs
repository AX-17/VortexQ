using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Vortex.Bot.Attributes;
using Vortex.Bot.Core.Service;
using Vortex.Protocol.Packets;

namespace Vortex.Bot.Command.Terraria;

[Command("进度", "progress")]
[HelpText("查看游戏进度")]
[CommandType(CommandType.Group)]
[Permission("vortex.terraria.progress")]
public static class GameProgressCommand
{
    [Main]
    public static async Task ShowGameProgress(GroupCommandArgs args)
    {
        TerrariaServerService? serverManager = args.Context.Server?.Services.GetService<TerrariaServerService>();
        if (serverManager == null)
        {
            await args.ReplyAsync("服务器管理器未初始化");
            return;
        }

        if (!serverManager.TryGetUserServer(args.SenderUin, args.GroupUin, out TerrariaServer? server) || server == null)
        {
            await args.ReplyAsync("请先使用 '切换 <名称>' 选择要操作的服务器!");
            return;
        }

        GameProgressPacketResponse? progress = await server.GetGameProgressAsync();

        if (progress?.Success == true && progress.Progress != null)
        {
            StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"[{server.Config.Name}] 游戏进度");
            sb.AppendLine();

            foreach (KeyValuePair<string, bool> item in progress.Progress)
            {
                var status = item.Value ? "✅" : "❌";
                sb.AppendLine($"{status} {item.Key}");
            }

            await args.ReplyAsync(sb.ToString());
        }
        else
        {
            await args.ReplyAsync($"[{server.Config.Name}] 获取进度失败: {progress?.Message ?? "无法连接服务器"}");
        }
    }
}
