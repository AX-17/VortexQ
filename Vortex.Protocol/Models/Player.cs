namespace Vortex.Protocol.Models;

public class Player
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public bool IsLogin { get; set; }
}
