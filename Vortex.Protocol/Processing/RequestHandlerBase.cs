using Vortex.Protocol.Interfaces;

namespace Vortex.Protocol.Processing;

public abstract class RequestHandlerBase<TRequest, TResponse>
    where TRequest : IServicePacket
    where TResponse : IClientPacket, new()
{
    public abstract TResponse Handle(TRequest request);

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
}
