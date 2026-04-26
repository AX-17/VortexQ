using Microsoft.Extensions.DependencyInjection;
using Vortex.Bot.Attributes;
using Vortex.Bot.Core.Service;
using Vortex.Protocol.Packets;

namespace Vortex.Bot.Command.Terraria;

[Command("重启服务器", "restart")]
[HelpText("重启游戏服务器")]
[CommandType(CommandType.Group)]
[Permission("vortex.terraria.server.restart")]
public static class ServerRestartCommand
{
    [Main]
    public static async Task RestartServer(GroupCommandArgs args)
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

        string startArgs = args.Params.Count > 0 ? string.Join(" ", args.Params) : "";
        ServerRestartPacketResponse? result = await server.RestartAsync(startArgs);

        if (result?.Success == true)
        {
            await args.ReplyAsync($"[{server.Config.Name}] 正在重启服务器，请稍后...");
        }
        else
        {
            await args.ReplyAsync($"[{server.Config.Name}] 重启失败: {result?.Message ?? "无法连接服务器"}");
        }
    }
}
