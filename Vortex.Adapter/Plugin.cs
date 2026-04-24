using Vortex.Adapter.DB;
using Vortex.Adapter.Net;
using Vortex.Adapter.Setting;
using Vortex.Protocol.Enums;
using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Models;
using Vortex.Protocol.Packets;
using System.Reflection;
using System.Threading.Channels;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Vortex.Adapter;

[ApiVersion(2, 1)]
public class Plugin : TerrariaPlugin
{
    public override string Author => "少司命";
    public override string Description => "Vortex 适配插件";
    public override string Name => Assembly.GetExecutingAssembly().GetName().Name!;
    public override Version Version => new Version(1, 0, 0, 0);

    internal static readonly List<TSPlayer> ServerPlayers = new();
    internal static Channel<int> Channeler = Channel.CreateBounded<int>(1);
    internal static readonly Dictionary<int, KillNpc> DamageBoss = [];

    private long _timerCount = 0;
    private readonly System.Timers.Timer _timer = new();

    internal static VortexClient? Client;
    internal static PlayerOnline Onlines = [];
    internal static PlayerDeath Deaths = [];
    private PacketHandler? _packetHandler;

    public Plugin(Main game) : base(game)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    public override void Initialize()
    {
        if (!Directory.Exists("world"))
        {
            Directory.CreateDirectory("world");
        }

        Client = new VortexClient(Config.Instance.SocketConfig);
        _packetHandler = new PacketHandler(Client);

        Utils.MapingCommand();

        Client.OnConnected += OnClientConnected;
        Client.OnDisconnected += OnClientDisconnected;

        ServerApi.Hooks.GamePostInitialize.Register(this, OnInit);
        ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);
        ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
        ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
        ServerApi.Hooks.ServerChat.Register(this, OnChat);
        ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
        ServerApi.Hooks.NpcSpawn.Register(this, OnSpawn);
        ServerApi.Hooks.NpcStrike.Register(this, OnStrike);
        ServerApi.Hooks.NpcKilled.Register(this, OnKillNpc);
        GetDataHandlers.KillMe.Register(this.OnKill);

        Config.Instance.SocketConfig.EmptyCommand.ForEach(x => Commands.ChatCommands.Add(new("", (_) => { }, x)));
        GeneralHooks.ReloadEvent += Config.Reload;
        Utils.HandleCommandLine(Environment.GetCommandLineArgs());

        _timer.AutoReset = true;
        _timer.Enabled = true;
        _timer.Interval = Config.Instance.SocketConfig.HeartBeatTimer;
        _timer.Elapsed += TimerUpdate;

        Client.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Client != null)
            {
                Client.OnConnected -= OnClientConnected;
                Client.OnDisconnected -= OnClientDisconnected;
                Client.Dispose();
            }

            ServerApi.Hooks.GamePostInitialize.Deregister(this, OnInit);
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreet);
            ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
            ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
            ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
            ServerApi.Hooks.NpcSpawn.Deregister(this, OnSpawn);
            ServerApi.Hooks.NpcStrike.Deregister(this, OnStrike);
            ServerApi.Hooks.NpcKilled.Deregister(this, OnKillNpc);
            GetDataHandlers.KillMe.UnRegister(this.OnKill);
            GeneralHooks.ReloadEvent -= Config.Reload;

            _timer.Elapsed -= TimerUpdate;
            _timer.Stop();

            RemoveAssemblyCommands(Assembly.GetExecutingAssembly());
        }
        base.Dispose(disposing);
    }

    public static void RemoveAssemblyCommands(Assembly assembly)
    {
        Commands.ChatCommands.RemoveAll(cmd => cmd.GetType().Assembly == assembly);
    }

    private void TimerUpdate(object? sender, ElapsedEventArgs e)
    {
        // 心跳包可以通过其他方式实现，或者使用专门的 PingPacket
    }

    private void OnClientConnected()
    {
        TShock.Log.ConsoleInfo("[Vortex.Adapter] 已连接到服务器");
    }

    private void OnClientDisconnected()
    {
        TShock.Log.ConsoleInfo("[Vortex.Adapter] 与服务器断开连接");
    }

    private void OnKillNpc(NpcKilledEventArgs args)
    {
        if (args.npc != null && args.npc.active && args.npc.boss)
        {
            if (DamageBoss.TryGetValue(args.npc.netID, out var killNpc))
            {
                killNpc.IsAlive = false;
                killNpc.KillTime = DateTime.Now;
            }
        }
    }

    private void OnStrike(NpcStrikeEventArgs args)
    {
        if (args.Npc != null && args.Npc.active && args.Npc.boss)
        {
            if (DamageBoss.TryGetValue(args.Npc.netID, out var killNpc) && killNpc != null)
            {
                var damage = killNpc.Strikes.Find(x => x.Player == args.Player.name);
                if (damage != null)
                {
                    damage.Damage += args.Damage;
                }
                else
                {
                    killNpc.Strikes.Add(new()
                    {
                        Player = args.Player.name,
                        Damage = args.Damage
                    });
                }
            }
        }
    }

    private void OnSpawn(NpcSpawnEventArgs args)
    {
        var npc = Main.npc[args.NpcId];
        if (npc != null && npc.active && npc.boss)
        {
            DamageBoss[npc.netID] = new()
            {
                Id = npc.netID,
                Name = npc.FullName,
                MaxLife = npc.lifeMax
            };
        }
    }

    private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        var resourceName = $"embedded.{new AssemblyName(args.Name).Name}.dll";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            var assemblyData = new byte[stream.Length];
            stream.ReadExactly(assemblyData);
            return Assembly.Load(assemblyData);
        }
        return null;
    }

    private void OnInit(EventArgs args)
    {
        if (Channeler.Reader.TryRead(out var mode))
        {
            Main.GameMode = mode;
            TSPlayer.All.SendData(PacketTypes.WorldInfo);
        }

        // 服务器初始化完成，可以发送通知
        TShock.Log.ConsoleInfo("[Vortex.Adapter] 服务器初始化完成");
    }

    private void OnChat(ServerChatEventArgs args)
    {
        var player = TShock.Players[args.Who];
        if (player != null && player.Active)
        {
            if (args.Text.StartsWith(TShock.Config.Settings.CommandSilentSpecifier)
                || args.Text.StartsWith(TShock.Config.Settings.CommandSpecifier))
            {
                var prefix = args.Text.StartsWith(TShock.Config.Settings.CommandSilentSpecifier)
                    ? TShock.Config.Settings.CommandSilentSpecifier
                    : TShock.Config.Settings.CommandSpecifier;

                // 发送玩家命令事件到服务器
                _ = Task.Run(async () =>
                {
                    if (Client?.IsConnected == true)
                    {
                        // 这里可以发送一个专门的 PlayerCommandPacket
                        // 暂时使用广播消息
                        await Client.SendPacketAsync(new BroadcastMessagePacket
                        {
                            Text = $"[命令] {player.Name}: {args.Text}",
                            Color = [255, 255, 255]
                        });
                    }
                });
            }
            else
            {
                // 发送玩家聊天事件到服务器
                _ = Task.Run(async () =>
                {
                    if (Client?.IsConnected == true)
                    {
                        await Client.SendPacketAsync(new BroadcastMessagePacket
                        {
                            Text = $"[{player.Group.Name}]{player.Name}: {args.Text}",
                            Color = [255, 255, 255]
                        });
                    }
                });
            }
        }
    }

    private void OnJoin(JoinEventArgs args)
    {
        var player = TShock.Players[args.Who];
        if (player != null)
        {
            if (Config.Instance.LimitJoin && TShock.UserAccounts.GetUserAccountByName(player.Name) == null)
            {
                player.Disconnect(string.Join("\n", Config.Instance.DisConnentFormat));
            }
        }
    }

    private void OnLeave(LeaveEventArgs args)
    {
        var player = TShock.Players[args.Who];
        if (player != null)
        {
            if (!ServerPlayers.Contains(player))
            {
                return;
            }

            ServerPlayers.Remove(player);
            Onlines.UpdateAll();

            // 发送玩家离开事件
            _ = Task.Run(async () =>
            {
                if (Client?.IsConnected == true)
                {
                    await Client.SendPacketAsync(new BroadcastMessagePacket
                    {
                        Text = $"{player.Name} 离开了服务器",
                        Color = [255, 255, 0]
                    });
                }
            });
        }
    }

    private void OnGreet(GreetPlayerEventArgs args)
    {
        var player = TShock.Players[args.Who];
        if (player != null && player.Active)
        {
            ServerPlayers.Add(player);

            // 发送玩家加入事件
            _ = Task.Run(async () =>
            {
                if (Client?.IsConnected == true)
                {
                    await Client.SendPacketAsync(new BroadcastMessagePacket
                    {
                        Text = $"{player.Name} 加入了服务器",
                        Color = [255, 255, 0]
                    });
                }
            });
        }
    }

    private void OnUpdate(EventArgs args)
    {
        _timerCount++;
        if (_timerCount % 60 == 0)
        {
            ServerPlayers.ForEach(p =>
            {
                if (p != null && p.Active)
                {
                    Onlines[p.Name] += 1;
                }
            });
        }
    }

    private void OnKill(object? sender, GetDataHandlers.KillMeEventArgs e)
    {
        Deaths.Add(e.Player.Name);
    }
}
