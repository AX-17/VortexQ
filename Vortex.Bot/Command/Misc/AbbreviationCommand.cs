using System.Text.Json.Nodes;
using Vortex.Bot.Attributes;

namespace Vortex.Bot.Command.Misc;

[Command("缩写", "abbr")]
[CommandType(CommandType.Group | CommandType.Friend)]
[Permission("vortex.misc.abbreviation")]
public static class AbbreviationCommand
{
    [Main]
    public static async Task Execute(CommandArgs args, [Param("缩写文本")] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            await args.ReplyAsync("请输入要查询的缩写");
            return;
        }

        try
        {
            using var client = new HttpClient();
            string url = $"https://oiapi.net/API/Nbnhhsh?text={Uri.EscapeDataString(text)}";
            string result = await client.GetStringAsync(url);

            JsonNode? data = JsonNode.Parse(result);
            JsonArray? trans = data?["data"]?[0]?["trans"]?.AsArray();

            if (trans != null && trans.Any())
            {
                var meanings = trans.Select(t => t?.ToString()).Where(s => !string.IsNullOrEmpty(s));
                await args.ReplyAsync($"缩写 `{text}` 可能为:\n{string.Join(", ", meanings)}");
            }
            else
            {
                await args.ReplyAsync("也许该缩写没有被收录!");
            }
        }
        catch (Exception ex)
        {
            await args.ReplyAsync($"查询失败: {ex.Message}");
        }
    }
}
