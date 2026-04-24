using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Packets;

public class ExecuteCommandPacket : IServicePacket
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public PacketType PacketID => PacketType.ExecuteCommand;

    public string Text { get; set; } = string.Empty;
}

public class ExecuteCommandPacketResponse : IClientPacket
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PacketType PacketID => PacketType.ExecuteCommandResponse;

    public List<string> Params { get; set; } = [];
}
