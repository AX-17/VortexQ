using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Packets;

public class ClientIdentityPacket : INetPacket
{
    public PacketType PacketID => PacketType.ClientIdentity;

    public Guid ClientId { get; set; } = Guid.NewGuid();

    public string ClientName { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}

public class ClientIdentityResponsePacket : IClientPacket
{
    public PacketType PacketID => PacketType.ClientIdentityResponse;
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public Guid ClientId { get; set; }

    public int SessionId { get; set; }
}
