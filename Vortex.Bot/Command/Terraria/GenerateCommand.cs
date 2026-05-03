using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Vortex.Bot.Attributes;
using Vortex.Bot.Core.Service;

namespace Vortex.Bot.Command.Terraria;

[Command("生成地图")]
[HelpText("生成Terraria服务器地图图片")]
[CommandType(CommandType.Group)]
[Permission("vortex.terraria.generate")]
[DefaultCommand]
public static class GenerateCommand
{
    [Main]
    public static async Task GenerateMap(GroupCommandArgs args)
    {
        var serverManager = args.Context.Server?.Services.GetService<TerrariaServerService>();
        if (serverManager == null)
        {
            await args.ReplyWithAtAsync("服务器管理器未初始化");
            return;
        }

        if (!serverManager.TryGetUserServer(args.SenderUin, args.GroupUin, out var server) || server == null)
        {
            await args.ReplyWithAtAsync("请先使用 '切换 <名称>' 选择要操作的服务器!");
            return;
        }
        try
        {
            var response = await server.GetMapImageAsync(Protocol.Packets.ImageType.Png);
            if(response == null || response.Success == false)
            {
                await args.ReplyWithAtAsync("生成地图失败: 服务器未响应");
                return;
            }
            await args.ReplyImageAsync(response.Buffer);
        }
        catch (Exception e)
        {
            await args.ReplyWithAtAsync($"生成地图失败: {e.Message}");
        }
    }
}
