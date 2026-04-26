using Microsoft.Extensions.DependencyInjection;
using Vortex.Bot.Attributes;
using Vortex.Bot.Core.Service;
using Vortex.Bot.Utility.Images;
using Vortex.Protocol.Packets;

namespace Vortex.Bot.Command.Terraria;

[Command("背包", "inventory", "inv", "bag")]
[HelpText("查看玩家背包")]
[CommandType(CommandType.Group)]
[Permission("vortex.terraria.inventory")]
public static class InventoryCommand
{
    [Main]
    public static async Task Execute(GroupCommandArgs args, [Param("玩家名称")] string playerName)
    {
        TerrariaServerService? serverManager = args.Context.Server?.Services.GetService<TerrariaServerService>();
        if (serverManager == null)
        {
            await args.ReplyAsync("服务器管理器未初始化");
            return;
        }

        if (!serverManager.TryGetUserServer(args.SenderUin, args.GroupUin, out TerrariaServer? server) || server == null)
        {
            await args.ReplyAsync("服务器不存在或未切换至一个服务器!");
            return;
        }

        await args.ReplyAsync($"正在查询 [{server.Config.Name}] 玩家 {playerName} 的背包...");

        PlayerInventoryPacketResponse? result = await server.QueryPlayerInventoryAsync(playerName);

        if (result?.Success == true && result.PlayerData != null)
        {
            try
            {
                // 生成背包图片
                var builder = InventoryGenerateExtensions.FromPlayerData(result.PlayerData, server.Config.Name);
                byte[] imageData = builder.Build();

                await args.ReplyImageAsync(imageData);
            }
            catch (Exception ex)
            {
                await args.ReplyAsync($"生成背包图片失败: {ex.Message}");
            }
        }
        else
        {
            await args.ReplyAsync($"查询失败: {result?.Message ?? "无法连接服务器或玩家不存在"}");
        }
    }
}
