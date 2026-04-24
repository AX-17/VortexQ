using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Packets;

public class WorldMapPacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();

    public PacketType PacketID => PacketType.GenerateWorldMap;
}

public class WorldMapPacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.GenerateWorldMapResponse;
    public byte[] Buffer { get; set; } = [];
}
