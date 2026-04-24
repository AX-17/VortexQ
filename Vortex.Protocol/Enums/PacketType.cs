namespace Vortex.Protocol.Enums;

public enum PacketType : byte
{
    // 认证相关
    ClientAuth,
    ClientAuthResponse,

    // 客户端身份
    ClientIdentity,
    ClientIdentityResponse,

    // 地图相关
    GenerateWorldMap,
    GenerateWorldMapResponse,

    // 消息广播
    BroadcastMessage,
    BroadcastMessageResponse,
    PrivateMessage,
    PrivateMessageResponse,

    // 服务器命令
    ExecuteCommand,
    ExecuteCommandResponse,

    // 玩家相关
    QueryPlayerInventory,
    QueryPlayerInventoryResponse,
    RegisterAccount,
    RegisterAccountResponse,
    ResetPlayerPassword,
    ResetPlayerPasswordResponse,
    QueryAccount,
    QueryAccountResponse,
    ExportPlayer,
    ExportPlayerResponse,

    // 服务器管理
    ResetServer,
    ResetServerResponse,
    RestartServer,
    RestartServerResponse,
    GetServerStatus,
    GetServerStatusResponse,

    // 排行榜和进度
    GetGameProgress,
    GetGameProgressResponse,
    GetDeathRank,
    GetDeathRankResponse,
    GetOnlineRank,
    GetOnlineRankResponse,
    GetServerOnline,
    GetServerOnlineResponse,
    GetPlayerStrikeBoss,
    GetPlayerStrikeBossResponse,

    // 地图和文件
    GetMapImage,
    GetMapImageResponse,
    UploadWorldFile,
    UploadWorldFileResponse,

    // 连接状态
    SocketConnectStatus,
    SocketConnectStatusResponse,
}
