namespace Vortex.Protocol.Interfaces;

public interface IClientPacket : INetPacket
{
    Guid RequestId { get; set; }
    bool Success { get; set; }
    string Message { get; set; }
}
