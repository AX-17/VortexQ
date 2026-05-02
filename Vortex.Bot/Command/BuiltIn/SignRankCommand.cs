using Vortex.Bot.Attributes;
using Vortex.Bot.Database.Models;
using Vortex.Bot.Utility.Images;

namespace Vortex.Bot.Command.BuiltIn;

[Command("签到排行")]
[Alias("signrank", "rank")]
[HelpText("查看签到排行榜")]
[CommandType(CommandType.Group | CommandType.Friend)]
[Permission("vortex.sign.rank")]
[DefaultCommand]
public static class SignRankCommand
{
    [Main]
    public static async Task ShowSignRank(CommandArgs args)
    {
        try
        {
            var signs = Sign.GetAll()
                .OrderByDescending(x => x.Date)
                .Take(10)
                .ToList();

            var builder = TableBuilder.Create()
                .SetHeader("排名", "账号", "累计天数")
                .SetTitle("签到排行")
                .SetMemberUin(args.SenderUin);

            int i = 1;
            foreach (var sign in signs)
            {
                builder.AddRow(i.ToString(), sign.UserId.ToString(), sign.Date.ToString());
                i++;
            }

            var imageData = builder.Build();
            await args.ReplyImageAsync(imageData);
        }
        catch (Exception e)
        {
            await args.ReplyWithAtAsync($"获取签到排行失败: {e.Message}");
        }
    }
}
