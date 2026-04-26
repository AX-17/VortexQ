using Microsoft.Extensions.DependencyInjection;
using Vortex.Bot.Attributes;
using Vortex.Bot.Core.Service;

namespace Vortex.Bot.Command.Terraria;

[Command("玩家信息", "ui", "userinfo")]
[CommandType(CommandType.Group)]
[Permission("vortex.terraria.userinfo")]
public static class TerrariaUserInfoCommand
{
    [Main]
    public static async Task Execute(GroupCommandArgs args, [Param("角色名称")] string characterName)
    {
        var serverManager = args.Context.Server?.Services.GetService<TerrariaServerService>();
        if (serverManager == null)
        {
            await args.ReplyAsync("服务器管理器未初始化");
            return;
        }

        if (!serverManager.TryGetUserServer(args.SenderUin, args.GroupUin, out var server) || server == null)
        {
            await args.ReplyAsync("服务器不存在或未切换至一个服务器!");
            return;
        }

        var result = await server.QueryAccountAsync(characterName);

        if (result?.Success == true && result.Accounts.Count > 0)
        {
            var account = result.Accounts.FirstOrDefault(a => a.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
            
            if (account == null)
            {
                await args.ReplyAsync($"[{server.Config.Name}] 未找到玩家: {characterName}");
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[{server.Config.Name}][{account.Name}] 账户信息:");
            sb.AppendLine($"ID: {account.ID}");
            sb.AppendLine($"权限组: {account.Group}");
            sb.AppendLine($"注册时间: {account.RegisterTime}");
            sb.AppendLine($"最后登录: {account.LastLoginTime}");
            sb.AppendLine($"IP: {account.IP}");
            
            await args.ReplyAsync(sb.ToString());
        }
        else
        {
            await args.ReplyAsync($"查询失败: {result?.Message ?? "无法连接服务器或玩家不存在"}");
        }
    }
}
