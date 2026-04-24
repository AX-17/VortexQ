using System.Net.Sockets;

namespace Vortex.Bot.Models;

/// <summary>
/// 客户端连接信息
/// </summary>
public class ClientConnection
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public TcpClient TcpClient { get; set; } = null!;
    public NetworkStream Stream => TcpClient.GetStream();
    public DateTime ConnectedAt { get; set; } = DateTime.Now;
    public int SessionId { get; set; }
    public bool IsAuthenticated { get; set; }
    public string Endpoint { get; set; } = string.Empty;
}
