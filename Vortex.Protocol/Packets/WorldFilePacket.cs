using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Packets;

public class WorldFilePacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.UploadWorldFile;
}

public class WorldFilePacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.UploadWorldFileResponse;

    public string WorldName { get; set; } = string.Empty;
    public byte[] WorldBuffer { get; set; } = Array.Empty<byte>();
}
