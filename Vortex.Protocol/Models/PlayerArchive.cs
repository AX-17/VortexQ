namespace Vortex.Protocol.Models;

public class PlayerArchive
{
    public byte[] Buffer { get; set; } = Array.Empty<byte>();
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}
