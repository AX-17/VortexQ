using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Packets;

public enum SocketConnectType : byte
{
    Success,
    VerifyError,
    ServerNull,
    Error
}

public class ConnectionStatusPacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.SocketConnectStatus;

    public SocketConnectType Status { get; set; }
}

public class ConnectionStatusPacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.SocketConnectStatusResponse;
}
