using System.Web;
using Vortex.Bot.Attributes;

namespace Vortex.Bot.Command.Misc;

[Command("wiki")]
[CommandType(CommandType.Group | CommandType.Friend)]
[Permission("vortex.misc.wiki")]
public static class WikiCommand
{
    [Main]
    public static async Task Execute(CommandArgs args, [Param("搜索内容(可选)")] string? searchTerm = null)
    {
        string baseUrl = "https://terraria.wiki.gg/zh/index.php";
        string url;

        if (string.IsNullOrEmpty(searchTerm))
        {
            url = baseUrl;
        }
        else
        {
            url = $"{baseUrl}?search={HttpUtility.UrlEncode(searchTerm)}";
        }

        await args.ReplyAsync(url);
    }
}
