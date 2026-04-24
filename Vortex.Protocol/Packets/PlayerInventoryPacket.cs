using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Models;

namespace Vortex.Protocol.Packets;

public class PlayerInventoryPacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.QueryPlayerInventory;

    public string Name { get; set; } = string.Empty;
}

public class PlayerInventoryPacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.QueryPlayerInventoryResponse;

    public PlayerData? PlayerData { get; set; }
}
