using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Models;

namespace Vortex.Protocol.Packets;

public class BossDamagePacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.GetPlayerStrikeBoss;
}

public class BossDamagePacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.GetPlayerStrikeBossResponse;

    public List<KillNpc> Damages { get; set; } = [];
}
