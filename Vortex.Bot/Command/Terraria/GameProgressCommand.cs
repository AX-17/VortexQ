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
            try
            {
                // 使用新的图片生成器生成进度图片
                ProgressBuilder builder = ProgressBuilder.Create()
                    .SetServerName(server.Config.Name)
                    .SetTitle("Boss 击杀进度")
                    .SetItemsPerRow(4)
                    .SetCardSize(260)
                    .SetCardSpacing(25);

                // 添加所有Boss进度
                foreach (KeyValuePair<string, bool> item in progress.Progress)
                {
                    string bossName = item.Key;
                    bool isKilled = item.Value;

                    // 尝试多种图片格式和路径
                    string[] possiblePaths = new[]
                    {
                        $"Resources/Boss/{bossName}.png",
                        $"Resources/Boss/{bossName}.jpg",
                        $"Resources/Boss/{bossName}.jpeg",
                        $"Resources/Terraria/Boss/{bossName}.png",
                        $"Resources/Terraria/Boss/{bossName}.jpg",
                    };

                    string? imagePath = possiblePaths.FirstOrDefault(File.Exists);

                    // 如果找不到图片，使用默认路径（生成器会处理缺失情况）
                    imagePath ??= $"Resources/Boss/{bossName}.png";

                    builder.AddBoss(bossName, imagePath, isKilled);
                }

                byte[] imageData = builder.Build();
                await args.ReplyImageAsync(imageData);
            }
            catch (Exception ex)
            {
                await args.ReplyAsync($"生成进度图片失败: {ex.Message}");
            }
        }
        else
        {
            await args.ReplyAsync($"[{server.Config.Name}] 获取进度失败: {progress?.Message ?? "无法连接服务器"}");
        }
    }
}
