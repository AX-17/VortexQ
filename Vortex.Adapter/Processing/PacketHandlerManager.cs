using System.Reflection;
using Vortex.Adapter.Net;
using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;
using TShockAPI;

namespace Vortex.Adapter.Processing;

/// <summary>
/// 数据包处理器管理器
/// 负责从程序集自动注册和管理所有数据包处理器
/// </summary>
public sealed class PacketHandlerManager
{
    private readonly VortexClient _client;
    private readonly Dictionary<PacketType, HandlerInfo> _handlers = new();

    /// <summary>
    /// 处理器信息
    /// </summary>
    private readonly struct HandlerInfo
    {
        public readonly Func<VortexClient, IRequestHandler> Factory;

        public HandlerInfo(Func<VortexClient, IRequestHandler> factory)
        {
            Factory = factory;
        }
    }

    public PacketHandlerManager(VortexClient client)
    {
        _client = client;
        RegisterHandlersFromAssembly();
    }

    /// <summary>
    /// 从当前程序集自动注册所有继承 RequestHandlerBase 的处理器
    /// </summary>
    private void RegisterHandlersFromAssembly()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && IsRequestHandler(t));

        foreach (var handlerType in handlerTypes)
        {
            RegisterHandlerType(handlerType);
        }

        TShock.Log.ConsoleInfo($"[Vortex.Adapter] 已自动注册 {_handlers.Count} 个数据包处理器");
    }

    /// <summary>
    /// 判断类型是否继承 RequestHandlerBase
    /// </summary>
    private static bool IsRequestHandler(Type type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(RequestHandlerBase<,>))
            {
                return true;
            }
            type = type.BaseType!;
        }
        return false;
    }

    /// <summary>
    /// 注册单个处理器类型
    /// </summary>
    private void RegisterHandlerType(Type handlerType)
    {
        try
        {
            // 获取 PacketType
            var packetType = GetPacketTypeFromHandlerType(handlerType);
            if (packetType == default)
            {
                TShock.Log.ConsoleError($"[Vortex.Adapter] 无法获取处理器 {handlerType.Name} 的 PacketType");
                return;
            }

            // 编译工厂方法
            var factory = CreateFactory(handlerType);

            _handlers[packetType] = new HandlerInfo(factory);
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[Vortex.Adapter] 注册处理器 {handlerType.Name} 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 从处理器类型获取 PacketType
    /// </summary>
    private static PacketType GetPacketTypeFromHandlerType(Type handlerType)
    {
        try
        {
            // 使用反射获取 PacketType 属性，避免创建实例
            var packetTypeProperty = handlerType.GetProperty("PacketType");
            if (packetTypeProperty != null)
            {
                // 创建临时实例获取 PacketType
                var tempClient = new VortexClient(new Setting.Configs.SocketConfig());
                var handler = (IRequestHandler)Activator.CreateInstance(handlerType, tempClient)!;
                var packetType = handler.PacketType;
                TShock.Log.ConsoleInfo($"[Vortex.Adapter] 处理器 {handlerType.Name} 的 PacketType: {packetType}");
                return packetType;
            }
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[Vortex.Adapter] 获取处理器 {handlerType.Name} 的 PacketType 失败: {ex.Message}");
        }
        return default;
    }

    /// <summary>
    /// 创建处理器工厂方法
    /// </summary>
    private static Func<VortexClient, IRequestHandler> CreateFactory(Type handlerType)
    {
        var constructor = handlerType.GetConstructor(new[] { typeof(VortexClient) })
            ?? throw new InvalidOperationException($"处理器 {handlerType.Name} 缺少 VortexClient 构造函数");

        return client => (IRequestHandler)constructor.Invoke(new object[] { client });
    }

    /// <summary>
    /// 处理数据包
    /// </summary>
    public async Task<bool> HandlePacketAsync(INetPacket packet)
    {
        if (!_handlers.TryGetValue(packet.PacketID, out var handlerInfo))
        {
            TShock.Log.ConsoleWarn($"[Vortex.Adapter] 未找到数据包 {packet.PacketID} 的处理器");
            return false;
        }

        // 检查包类型
        if (packet is not IServicePacket servicePacket)
        {
            TShock.Log.ConsoleError($"[Vortex.Adapter] 数据包 {packet.PacketID} 不是 IServicePacket 类型");
            return false;
        }

        try
        {
            // 创建处理器实例并处理请求
            var handler = handlerInfo.Factory(_client);
            TShock.Log.ConsoleInfo($"[Vortex.Adapter] 正在处理数据包 {packet.PacketID}");
            var response = handler.Handle(servicePacket);

            // 发送响应
            if (response != null)
            {
                await _client.SendPacketAsync(response);
                TShock.Log.ConsoleInfo($"[Vortex.Adapter] 已发送响应 {response.PacketID}");
            }

            return true;
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[Vortex.Adapter] 处理数据包 {packet.PacketID} 时出错: {ex}");
            return false;
        }
    }

    /// <summary>
    /// 获取已注册的处理器数量
    /// </summary>
    public int HandlerCount => _handlers.Count;
}
