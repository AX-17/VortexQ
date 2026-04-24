using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Packets;

public class PasswordResetPacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.ResetPlayerPassword;

    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class PasswordResetPacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.ResetPlayerPasswordResponse;
}
