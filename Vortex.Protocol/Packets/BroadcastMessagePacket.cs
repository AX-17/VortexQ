using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Packets;

public class BroadcastMessagePacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.BroadcastMessage;

    public string Text { get; set; } = string.Empty;
    public byte[] Color { get; set; } = [];
}

public class BroadcastMessagePacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.BroadcastMessageResponse;
}
