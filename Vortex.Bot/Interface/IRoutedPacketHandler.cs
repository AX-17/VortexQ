using Vortex.Bot.Models;
using Vortex.Protocol.Interfaces;

namespace Vortex.Bot.Interface;

/// <summary>
/// 带路由上下文的包处理器接口
/// </summary>
public interface IRoutedPacketHandler<in TRequest, TResponse>
    where TRequest : IServicePacket
    where TResponse : IClientPacket, new()
{
    Task<TResponse> HandleAsync(TRequest request, PacketRouteContext context);
}
