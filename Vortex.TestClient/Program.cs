using Vortex.Protocol.Packets;
using Vortex.TestClient;

Console.WriteLine("=== Vortex Test Client ===");
Console.WriteLine();

// 配置
var serverAddress = "127.0.0.1";
var serverPort = 7778;
var clientName = $"TestClient_{Guid.NewGuid().ToString()[..8]}";

// 创建测试客户端
using var client = new TestClient(serverAddress, serverPort, clientName);

// 订阅事件
client.OnLog += msg => { };
client.OnConnected += () => Console.WriteLine("[Event] Connected to server");
client.OnDisconnected += () => Console.WriteLine("[Event] Disconnected from server");
client.OnPacketReceived += packet =>
{
    Console.WriteLine($"[Event] Packet received: {packet.PacketID}");
};

// 连接服务器
Console.WriteLine($"Connecting to {serverAddress}:{serverPort}...");
if (!await client.ConnectAsync())
{
    Console.WriteLine("Failed to connect. Press any key to exit.");
    Console.ReadKey();
    return;
}

// 身份验证
Console.WriteLine("Authenticating...");
if (!await client.AuthenticateAsync())
{
    Console.WriteLine("Authentication failed. Press any key to exit.");
    Console.ReadKey();
    return;
}

Console.WriteLine();
Console.WriteLine("=== Test Menu ===");
Console.WriteLine("1. Send Broadcast Message");
Console.WriteLine("2. Send Private Message");
Console.WriteLine("3. Execute Command");
Console.WriteLine("4. Query Server Status");
Console.WriteLine("5. Query Player Inventory");
Console.WriteLine("6. Get Game Progress");
Console.WriteLine("7. Get Online Rank");
Console.WriteLine("8. Get Death Rank");
Console.WriteLine("9. Get Server Online");
Console.WriteLine("0. Disconnect and Exit");
Console.WriteLine();

while (client.IsConnected && client.IsAuthenticated)
{
    Console.Write("Select option: ");
    var key = Console.ReadKey();
    Console.WriteLine();

    switch (key.KeyChar)
    {
        case '1':
            await TestBroadcastMessage(client);
            break;
        case '2':
            await TestPrivateMessage(client);
            break;
        case '3':
            await TestExecuteCommand(client);
            break;
        case '4':
            await TestServerStatus(client);
            break;
        case '5':
            await TestPlayerInventory(client);
            break;
        case '6':
            await TestGameProgress(client);
            break;
        case '7':
            await TestOnlineRank(client);
            break;
        case '8':
            await TestDeathRank(client);
            break;
        case '9':
            await TestServerOnline(client);
            break;
        case '0':
            client.Disconnect();
            return;
        default:
            Console.WriteLine("Invalid option");
            break;
    }

    Console.WriteLine();
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

// 测试方法
static async Task TestBroadcastMessage(TestClient client)
{
    Console.Write("Enter message: ");
    var message = Console.ReadLine() ?? "Hello from TestClient!";

    var packet = new BroadcastMessagePacket
    {
        Text = message,
        Color = [255, 255, 255] // White color
    };

    await client.SendPacketAsync(packet);
    Console.WriteLine("Broadcast message sent!");
}

static async Task TestPrivateMessage(TestClient client)
{
    Console.Write("Enter target player name: ");
    var target = Console.ReadLine() ?? "Player";
    Console.Write("Enter message: ");
    var message = Console.ReadLine() ?? "Hello!";

    var packet = new PrivateMessagePacket
    {
        Name = target,
        Text = message,
        Color = [255, 255, 0] // Yellow color
    };

    await client.SendPacketAsync(packet);
    Console.WriteLine("Private message sent!");
}

static async Task TestExecuteCommand(TestClient client)
{
    Console.Write("Enter command: ");
    var command = Console.ReadLine() ?? "/help";

    var request = new ExecuteCommandPacket
    {
        Text = command
    };

    var response = await client.RequestAsync<ExecuteCommandPacket, ExecuteCommandPacketResponse>(request);
    if (response != null)
    {
        Console.WriteLine($"Command executed: {response.Success}");
        Console.WriteLine($"Message: {response.Message}");
        if (response.Params.Count > 0)
        {
            Console.WriteLine("Output:");
            foreach (var param in response.Params)
            {
                Console.WriteLine($"  {param}");
            }
        }
    }
    else
    {
        Console.WriteLine("No response received");
    }
}

static async Task TestServerStatus(TestClient client)
{
    var request = new ServerStatusPacket();
    var response = await client.RequestAsync<ServerStatusPacket, ServerStatusPacketResponse>(request);

    if (response != null)
    {
        Console.WriteLine("=== Server Status ===");
        Console.WriteLine($"World Name: {response.WorldName}");
        Console.WriteLine($"World Size: {response.WorldWidth} x {response.WorldHeight}");
        Console.WriteLine($"World Mode: {response.WorldMode}");
        Console.WriteLine($"World ID: {response.WorldID}");
        Console.WriteLine($"World Seed: {response.WorldSeed}");
        Console.WriteLine($"Run Time: {response.RunTime}");
        Console.WriteLine($"TShock Path: {response.TShockPath}");
        Console.WriteLine($"Plugins Count: {response.Plugins.Count}");
    }
    else
    {
        Console.WriteLine("No response received");
    }
}

static async Task TestPlayerInventory(TestClient client)
{
    Console.Write("Enter player name: ");
    var playerName = Console.ReadLine() ?? "Player";

    var request = new PlayerInventoryPacket
    {
        Name = playerName
    };

    var response = await client.RequestAsync<PlayerInventoryPacket, PlayerInventoryPacketResponse>(request);

    if (response != null)
    {
        if (response.PlayerData != null)
        {
            Console.WriteLine($"=== Player: {response.PlayerData.Username} ===");
            Console.WriteLine($"Online: {response.PlayerData.OnlineStatus}");
            Console.WriteLine($"Health: {response.PlayerData.StatLife}/{response.PlayerData.StatLifeMax}");
            Console.WriteLine($"Mana: {response.PlayerData.StatMana}/{response.PlayerData.StatManaMax}");
            Console.WriteLine($"Inventory Items: {response.PlayerData.Inventory.Length}");
        }
        else
        {
            Console.WriteLine("Player not found or offline");
        }
    }
    else
    {
        Console.WriteLine("No response received");
    }
}

static async Task TestGameProgress(TestClient client)
{
    var request = new GameProgressPacket();
    var response = await client.RequestAsync<GameProgressPacket, GameProgressPacketResponse>(request);

    if (response != null)
    {
        Console.WriteLine("=== Game Progress ===");
        foreach (var (key, value) in response.Progress)
        {
            Console.WriteLine($"  {key}: {(value ? "✓" : "✗")}");
        }
    }
    else
    {
        Console.WriteLine("No response received");
    }
}

static async Task TestOnlineRank(TestClient client)
{
    var request = new OnlineRankPacket();
    var response = await client.RequestAsync<OnlineRankPacket, OnlineRankPacketResponse>(request);

    if (response != null)
    {
        Console.WriteLine("=== Online Rank ===");
        var sorted = response.OnlineRank.OrderByDescending(x => x.Value);
        foreach (var (name, time) in sorted)
        {
            Console.WriteLine($"  {name}: {TimeSpan.FromMinutes(time):hh\\:mm\\:ss}");
        }
    }
    else
    {
        Console.WriteLine("No response received");
    }
}

static async Task TestDeathRank(TestClient client)
{
    var request = new DeathRankPacket();
    var response = await client.RequestAsync<DeathRankPacket, DeathRankPacketResponse>(request);

    if (response != null)
    {
        Console.WriteLine("=== Death Rank ===");
        var sorted = response.Rank.OrderByDescending(x => x.Value);
        int rank = 1;
        foreach (var (name, count) in sorted)
        {
            Console.WriteLine($"  {rank}. {name}: {count} deaths");
            rank++;
        }
    }
    else
    {
        Console.WriteLine("No response received");
    }
}

static async Task TestServerOnline(TestClient client)
{
    var request = new ServerOnlinePacket();
    var response = await client.RequestAsync<ServerOnlinePacket, ServerOnlinePacketResponse>(request);

    if (response != null)
    {
        Console.WriteLine("=== Server Online ===");
        Console.WriteLine($"Players: {response.OnlineCount}/{response.MaxCount}");
        foreach (var player in response.Players)
        {
            Console.WriteLine($"  - {player.Name} [{player.Group}] {(player.IsLogin ? "(Logged In)" : "(Guest)")}");
        }
    }
    else
    {
        Console.WriteLine("No response received");
    }
}
