namespace Vortex.Bot.Configuration;

public class SocketConfiguration
{
    public bool Enabled { get; set; } = true;

    public ushort Port { get; set; } = 7778;

    public string Token { get; set; } = "";
}
