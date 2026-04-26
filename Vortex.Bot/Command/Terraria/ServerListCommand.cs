using Microsoft.Extensions.DependencyInjection;
using Vortex.Bot.Attributes;
using Vortex.Bot.Core.Service;
using Vortex.Bot.Utility.Images;
using Vortex.Protocol.Packets;

namespace Vortex.Bot.Command.Terraria;

[Command("服务器列表", "servers", "svlist")]
[HelpText("查看所有服务器列表")]
[CommandType(CommandType.Group)]
[Permission("vortex.terraria.server.list")]
public static class ServerListCommand
{
    [Main]
    public static async Task ShowServerList(GroupCommandArgs args)
    {
        TerrariaServerService? serverManager = args.Context.Server?.Services.GetService<TerrariaServerService>();
        if (serverManager == null)
        {
            await args.ReplyAsync("服务器管理器未初始化");
            return;
        }

        List<TerrariaServer> servers = args.GroupUin > 0
            ? [.. serverManager.GetServersByGroup(args.GroupUin)]
            : serverManager.GetAllServers().ToList();

        if (servers.Count == 0)
        {
            await args.ReplyAsync("此群未配置任何服务器!");
            return;
        }

        TableBuilder tableBuilder = new TableBuilder()
            .SetHeader("服务器名称", "IP", "端口", "版本", "说明", "状态", "世界", "种子", "大小")
            .SetTitle("服务器列表")
            .SetMemberUin((uint)args.SenderUin);

        foreach (TerrariaServer? server in servers)
        {
            ServerStatusPacketResponse? status = await server.GetStatusAsync();
            bool isOnline = status != null && status.Success;

            tableBuilder.AddRow(
                server.Config.Name,
                server.Config.IP,
                server.Config.DisplayPort.ToString(),
                server.Config.Version,
                server.Config.Describe,
                !isOnline ? "离线" : $"运行:{status!.RunTime:dd\\.hh\\:mm\\:ss}",
                !isOnline ? "-" : status.WorldName,
                !isOnline ? "-" : status.WorldSeed,
                !isOnline ? "-" : $"{status.WorldWidth}x{status.WorldHeight}"
            );
        }

        await args.ReplyImageAsync(tableBuilder.Build());
    }
}
