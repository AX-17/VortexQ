namespace Vortex.Bot.Models;

/// <summary>
/// 包路由上下文 - 包含发送者信息
/// </summary>
public class PacketRouteContext
{
    /// <summary>
    /// 发送者客户端ID
    /// </summary>
    public Guid SenderClientId { get; set; }

    /// <summary>
    /// 发送者会话ID
    /// </summary>
    public int SenderSessionId { get; set; }

    /// <summary>
    /// 发送者连接信息
    /// </summary>
    public ClientConnection SenderConnection { get; set; } = null!;

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime ReceiveTime { get; set; } = DateTime.Now;
}
