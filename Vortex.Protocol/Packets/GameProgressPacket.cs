using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Packets;

public class GameProgressPacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.GetGameProgress;
}

public class GameProgressPacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.GetGameProgressResponse;

    public Dictionary<string, bool> Progress { get; set; } = [];
}
