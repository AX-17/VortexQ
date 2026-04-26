using Vortex.Bot.Attributes;
using Vortex.Bot.Database.Models;
using Vortex.Bot.Utility.Images;

namespace Vortex.Bot.Command.BuiltIn;

[Command("me", "我的信息", "info")]
[HelpText("查看个人信息")]
[CommandType(CommandType.Group | CommandType.Friend)]
[Permission("vortex.selfinfo")]
public static class SelfInfoCommand
{
    [Main]
    public static async Task ShowInfo(GroupCommandArgs args)
    {
        var currency = Currency.Query(args.SenderUin);
        var sign = Sign.Query(args.SenderUin);

        var builder = ProfileItemBuilder.Create()
            .SetMemberUin(args.SenderUin)
            .SetTitle("个人信息")
            .AddItem("QQ", args.SenderUin.ToString())
            .AddItem("昵称", args.SenderDisplayName ?? "未知")
            .AddItem("权限组", args.Group.Name)
            .AddItem("金币", (currency?.Num ?? 0).ToString())
            .AddItem("连续签到", $"{sign?.Date ?? 0} 天");

        var imageData = builder.Build();
        await args.ReplyImageAsync(imageData);
    }

    [Main]
    public static async Task ShowInfo(PrivateCommandArgs args)
    {
        var currency = Currency.Query(args.SenderUin);
        var sign = Sign.Query(args.SenderUin);

        var builder = ProfileItemBuilder.Create()
            .SetMemberUin(args.SenderUin)
            .SetTitle("个人信息")
            .AddItem("QQ", args.SenderUin.ToString())
            .AddItem("昵称", args.FriendNickname ?? "未知")
            .AddItem("权限组", args.Group.Name)
            .AddItem("金币", (currency?.Num ?? 0).ToString())
            .AddItem("连续签到", $"{sign?.Date ?? 0} 天");

        var imageData = builder.Build();
        await args.ReplyImageAsync(imageData);
    }
}
