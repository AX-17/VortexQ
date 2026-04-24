using Newtonsoft.Json;

namespace Vortex.Adapter.Setting.Configs;

public class SocketConfig
{
    [JsonProperty("服务器地址")]
    public string IP = "127.0.0.1";

    [JsonProperty("服务器名称")]
    public string ServerName = "TerrariaServer";

    [JsonProperty("端口")]
    public int Port = 6000;

    [JsonProperty("验证令牌")]
    public string Token { get; set; } = string.Empty;

    [JsonProperty("心跳包间隔")]
    public int HeartBeatTimer = 1 * 60 * 1000;

    [JsonProperty("重连间隔")]
    public int ReConnectTimer = 5 * 1000;

    [JsonProperty("空指令注册")]
    public HashSet<string> EmptyCommand = ["购买", "抽"];
}
