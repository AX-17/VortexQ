using Vortex.Protocol.Enums;

namespace Vortex.Protocol.Interfaces;

public interface INetPacket
{
    PacketType PacketID { get; }
}