using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Packets;

public enum ImageType : byte
{
    Jpg,
    Png
}

public class MapImagePacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.GetMapImage;

    public ImageType ImageType { get; set; }
}

public class MapImagePacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.GetMapImageResponse;

    public byte[] Buffer { get; set; } = Array.Empty<byte>();
}
