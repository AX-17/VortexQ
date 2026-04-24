using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Packets;

public class ClientAuthPacket : INetPacket
{
    public PacketType PacketID => PacketType.ClientAuth;

    public string Token { get; set; } = string.Empty;
}

public class ClientAuthResponsePacket : IClientPacket
{
    public PacketType PacketID => PacketType.ClientAuthResponse;
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
