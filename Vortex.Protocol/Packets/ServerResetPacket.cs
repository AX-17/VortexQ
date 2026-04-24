using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Packets;

public class ServerResetPacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.ResetServer;

    public List<string> ResetCommand { get; set; } = new();
    public string StartArgs { get; set; } = string.Empty;
    public bool UseFile { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[] FileBuffer { get; set; } = Array.Empty<byte>();
}

public class ServerResetPacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.ResetServerResponse;
}
