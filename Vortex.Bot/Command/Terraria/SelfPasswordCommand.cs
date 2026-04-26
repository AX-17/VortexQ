using Microsoft.Extensions.DependencyInjection;
using Vortex.Bot.Attributes;
using Vortex.Bot.Core.Service;
using Vortex.Bot.Database.Models;

namespace Vortex.Bot.Command.Terraria;

[Command("我的密码", "mypwd", "password")]
[HelpText("查看自己的游戏密码")]
[CommandType(CommandType.Group)]
[Permission("vortex.terraria.selfpassword")]
public static class SelfPasswordCommand
{
    [Main]
    public static async Task Execute(GroupCommandArgs args)
    {
        var serverManager = args.Context.Server?.Services.GetService<TerrariaServerService>();
        if (serverManager == null)
        {
            await args.ReplyAsync("服务器管理器未初始化");
            return;
        }

        if (!serverManager.TryGetUserServer(args.SenderUin, args.GroupUin, out var server) || server == null)
        {
            await args.ReplyAsync("服务器无效或未切换至一个有效服务器!");
            return;
        }

        var users = TerrariaUser.GetUsersById(args.SenderUin, server.Config.Name);

        if (users.Count == 0)
        {
            await args.ReplyAsync($"{server.Config.Name}上未找到你的注册信息。");
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[{server.Config.Name}] 你的注册密码:");

        foreach (var user in users)
        {
            sb.AppendLine($"人物: {user.Name}  密码: {user.Password}");
        }

        await args.ReplyAsync(sb.ToString());
    }
}
