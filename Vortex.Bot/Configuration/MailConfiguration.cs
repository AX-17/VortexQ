namespace Vortex.Bot.Configuration;

public class MailConfiguration
{
    public bool Enabled { get; set; } = false;

    public string Host { get; set; } = "smtp.qq.com";

    public int Port { get; set; } = 587;

    public string Password { get; set; } = "";

    public string From { get; set; } = "";

    public string FromName { get; set; } = "Vortex Bot";

    public bool EnableSsl { get; set; } = true;
}
