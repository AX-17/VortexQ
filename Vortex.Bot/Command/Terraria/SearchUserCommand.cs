using Microsoft.Extensions.DependencyInjection;
using Vortex.Bot.Attributes;
using Vortex.Bot.Core.Service;
using Vortex.Bot.Database.Models;

namespace Vortex.Bot.Command.Terraria;

[Command("搜索用户", "searchuser", "finduser")]
[HelpText("搜索服务器用户")]
[CommandType(CommandType.Group)]
[Permission("vortex.terraria.searchuser")]
public static class SearchUserCommand
{
    [Main]
    public static async Task Execute(GroupCommandArgs args, [Param("关键词")] string keyword)
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

        var result = await server.ExecuteCommandAsync($"/user list");

        if (result?.Success == true && result.Params != null)
        {
            var matches = result.Params.Where(p => p.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

            if (matches.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"[{server.Config.Name}] 搜索结果 (关键词: {keyword}):");
                foreach (var match in matches.Take(20))
                {
                    sb.AppendLine(match);
                }
                if (matches.Count > 20)
                {
                    sb.AppendLine($"...还有 {matches.Count - 20} 个结果");
                }
                await args.ReplyAsync(sb.ToString());
            }
            else
            {
                await args.ReplyAsync($"未找到包含 '{keyword}' 的用户");
            }
        }
        else
        {
            await args.ReplyAsync($"搜索失败: {result?.Message ?? "无法连接服务器"}");
        }
    }
}
