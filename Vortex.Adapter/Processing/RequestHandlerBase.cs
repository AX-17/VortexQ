using Vortex.Adapter.Net;
using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;

namespace Vortex.Adapter.Processing;

public interface IRequestHandler
{
    PacketType PacketType { get; }

    IClientPacket Handle(IServicePacket request);
}

public abstract class RequestHandlerBase<TRequest, TResponse>(VortexClient client) : IRequestHandler
    where TRequest : IServicePacket, new()
    where TResponse : IClientPacket, new()
{
    protected VortexClient Client { get; } = client;

    public PacketType PacketType { get; } = GetPacketType();

    private static PacketType GetPacketType()
    {
        return InstanceCache.PacketType;
    }

    private static class InstanceCache
    {
        public static readonly PacketType PacketType;

        static InstanceCache()
        {
            try
            {
                var instance = new TRequest();
                PacketType = instance.PacketID;
            }
            catch
            {
                PacketType = default;
            }
        }
    }

    public abstract TResponse Handle(TRequest request);

    public IClientPacket Handle(IServicePacket request)
    {
        return Handle((TRequest)request);
    }

    protected static TResponse CreateSuccessResponse(TRequest request, string message = "Success")
    {
        return new TResponse
        {
            RequestId = request.RequestId,
            Success = true,
            Message = message
        };
    }

    protected static TResponse CreateFailureResponse(TRequest request, string message)
    {
        return new TResponse
        {
            RequestId = request.RequestId,
            Success = false,
            Message = message
        };
    }

    protected async Task SendPacketAsync(INetPacket packet)
    {
        await Client.SendPacketAsync(packet);
    }
}
