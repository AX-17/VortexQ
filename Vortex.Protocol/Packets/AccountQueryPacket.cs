using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Models;

namespace Vortex.Protocol.Packets;

public class AccountQueryPacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.QueryAccount;

    public string? Target { get; set; }
}

public class AccountQueryPacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.QueryAccountResponse;

    public List<Account> Accounts { get; set; } = [];
}
