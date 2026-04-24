namespace Vortex.Protocol.Interfaces;

public interface IServicePacket : INetPacket
{
    Guid RequestId { get; set; }
}
