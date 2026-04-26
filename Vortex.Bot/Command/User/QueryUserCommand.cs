using Lagrange.Core.Message.Entities;
using System.Text;
using Vortex.Bot.Attributes;
using Vortex.Bot.Database.Models;

namespace Vortex.Bot.Command.User;

[Command("查", "query")]
[HelpText("查询用户信息")]
[CommandType(CommandType.Group)]
[Permission("vortex.user.query")]
public static class QueryUserCommand
{
    [Main]
    public static async Task Execute(GroupCommandArgs args, [Param("QQ号(可选)")] long? targetUserId = null)
    {
        long userId;

        MentionEntity? mention = args.MessageChain?.OfType<MentionEntity>().FirstOrDefault();
        if (mention != null)
        {
            userId = mention.Uin;
        }
        else if (targetUserId.HasValue)
        {
            userId = targetUserId.Value;
        }
        else if (args.Params.Count == 0)
        {
            userId = args.SenderUin;
        }
        else
        {
            await args.ReplyAsync("请@要查询的成员或输入QQ号");
            return;
        }

        Account account = Account.GetOrDefault(userId);
        Currency? currency = Currency.Query(userId);
        Sign? sign = Sign.Query(userId);

        var isSelf = userId == args.SenderUin;
        StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine(isSelf ? "你的信息:" : $"用户 {userId} 的信息:");
        sb.AppendLine($"QQ: {userId}");
        sb.AppendLine($"权限组: {account.Group.Name}");
        sb.AppendLine($"金币: {currency?.Num ?? 0}");
        sb.AppendLine($"连续签到: {sign?.Date ?? 0} 天");

        await args.ReplyAsync(sb.ToString());
    }
}
