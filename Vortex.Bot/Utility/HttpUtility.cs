using System.Collections.Specialized;
using System.Text;
using System.Text.Json;

namespace Vortex.Bot.Utility;

public static class HttpUtility
{
    private static readonly HttpClient HttpClient = new();
    public static async Task<string> GetStringAsync(string url, Dictionary<string, string>? args = null, CancellationToken cancellationToken = default)
    {
        return Encoding.UTF8.GetString(await GetByteAsync(url, args, cancellationToken));
    }

    public static string QueryUri(string url, Dictionary<string, string>? @params = null)
    {
        var uri = new UriBuilder(url);
        NameValueCollection args = System.Web.HttpUtility.ParseQueryString(uri.Query);
        if (@params is not null)
            foreach ((string? key, string? value) in @params)
                args[key] = value;
        uri.Query = args.ToString();
        return uri.ToString();
    }

    public static async Task<byte[]> GetByteAsync(string url, Dictionary<string, string>? args = null, CancellationToken cancellationToken = default)
    {
        return await HttpClient.GetByteArrayAsync(QueryUri(url, args), cancellationToken);
    }

    public static async Task<Stream> GetStreamAsync(string url, Dictionary<string, string>? args = null, CancellationToken cancellationToken = default)
    {
        return await HttpClient.GetStreamAsync(QueryUri(url, args), cancellationToken);
    }

    public static async Task<string> PostAsync(string url, Dictionary<string, string>? args = null, CancellationToken cancellationToken = default)
    {
        FormUrlEncodedContent form = new(args ?? []);
        HttpResponseMessage content = await HttpClient.PostAsync(url, form, cancellationToken);
        return await content.Content.ReadAsStringAsync(cancellationToken);
    }

    public static async Task<string> PostContentAsync(string url, Dictionary<string, string> args, CancellationToken cancellationToken = default)
    {
        StringContent payload = new(JsonSerializer.Serialize(args));
        HttpResponseMessage content = await HttpClient.PostAsync(url, payload, cancellationToken);
        return await content.Content.ReadAsStringAsync();
    }
}
