using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Models;

namespace Vortex.Protocol.Packets;

public class ServerStatusPacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.GetServerStatus;
}

public class ServerStatusPacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.GetServerStatusResponse;

    public string WorldName { get; set; } = string.Empty;
    public int WorldWidth { get; set; }
    public int WorldHeight { get; set; }
    public int WorldMode { get; set; }
    public int WorldID { get; set; }
    public string WorldSeed { get; set; } = string.Empty;
    public TimeSpan RunTime { get; set; }
    public string TShockPath { get; set; } = string.Empty;
    public List<Plugin> Plugins { get; set; } = new();
}
