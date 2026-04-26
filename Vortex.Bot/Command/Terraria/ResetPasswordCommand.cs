using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Vortex.Bot.Attributes;
using Vortex.Bot.Core.Service;
using Vortex.Bot.Database.Models;

namespace Vortex.Bot.Command.Terraria;

[Command("重置密码", "resetpwd")]
[CommandType(CommandType.Group)]
[Permission("vortex.terraria.resetpassword")]
public static class ResetPasswordCommand
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

        try
        {
            var users = TerrariaUser.GetUsersById(args.SenderUin, server.Config.Name);

            if (users.Count == 0)
            {
                await args.ReplyAsync($"{server.Config.Name}上未找到你的注册信息。");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[{server.Config.Name}] 密码重置成功!");

            foreach (var user in users)
            {
                string newPassword = Guid.NewGuid().ToString()[..8];

                var result = await server.ExecuteCommandAsync($"/user password {user.Name} {newPassword}");

                if (result?.Success == true)
                {
                    TerrariaUser.ResetPassword(args.SenderUin, server.Config.Name, user.Name, newPassword);
                    sb.AppendLine($"人物 {user.Name} 的新密码: {newPassword}");
                }
                else
                {
                    await args.ReplyAsync($"无法连接到服务器更改密码: {result?.Message ?? "未知错误"}");
                    return;
                }
            }

            await args.ReplyAsync(sb.ToString());
        }
        catch (Exception ex)
        {
            await args.ReplyAsync($"重置密码失败: {ex.Message}");
        }
    }
}
