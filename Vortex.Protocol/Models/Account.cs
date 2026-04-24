namespace Vortex.Protocol.Models;

public class Account
{
    public string Name { get; set; } = string.Empty;
    public string IP { get; set; } = string.Empty;
    public int ID { get; set; }
    public string Group { get; set; } = string.Empty;
    public string UUID { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RegisterTime { get; set; } = string.Empty;
    public string LastLoginTime { get; set; } = string.Empty;
}
