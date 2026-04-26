using Microsoft.Extensions.DependencyInjection;
using Vortex.Bot.Attributes;
using Vortex.Bot.Core.Service;
using Vortex.Bot.Utility.Images;
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
            await args.ReplyWithAtAsync("服务器管理器未初始化");
            return;
        }

        if (!serverManager.TryGetUserServer(args.SenderUin, args.GroupUin, out TerrariaServer? server) || server == null)
        {
            await args.ReplyWithAtAsync("请先使用 '切换 <名称>' 选择要操作的服务器!");
            return;
        }

        GameProgressPacketResponse? progress = await server.GetGameProgressAsync();

        if (progress?.Success == true && progress.Progress != null)
        {
            try
            {
                ProgressBuilder builder = ProgressBuilder.Create()
                    .SetServerName(server.Config.Name)
                    .SetTitle("Boss 击杀进度")
                    .SetItemsPerRow(4)
                    .SetCardSize(260)
                    .SetCardSpacing(25);

                foreach (var (bossName, isKilled) in progress.Progress)
                {
                    var imagePath = $"Resources/Boss/{bossName}.jpg";
                    builder.AddBoss(bossName, imagePath, isKilled);
                }

                byte[] imageData = builder.Build();
                await args.ReplyImageAsync(imageData);
            }
            catch (Exception ex)
            {
                await args.ReplyWithAtAsync($"生成进度图片失败: {ex.Message}");
            }
        }
        else
        {
            await args.ReplyWithAtAsync($"[{server.Config.Name}] 获取进度失败: {progress?.Message ?? "无法连接服务器"}");
        }
    }
}
