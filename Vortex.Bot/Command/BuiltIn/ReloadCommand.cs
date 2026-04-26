using Vortex.Bot.Attributes;
using Vortex.Bot.Events;

namespace Vortex.Bot.Command.BuiltIn;

[Command("reload")]
[HelpText("重载配置文件")]
[Permission("vortex.admin")]
public static class ReloadCommand
{
    [Main]
    public static async Task Reload(GroupCommandArgs args)
    {
        await args.ReplyAsync("正在重载配置...");
        await ReloadEvents.TriggerReloadAsync(args.SenderUin);
        await args.ReplyAsync($"配置重载完成，耗时: {ReloadEvents.LastElapsedMilliseconds}ms");
    }
}
