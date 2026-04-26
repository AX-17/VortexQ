using Vortex.Bot.Attributes;
using Vortex.Bot.Database.Models;

namespace Vortex.Bot.Command.Admin;

[Command("group", "权限组")]
[HelpText("管理权限组")]
[CommandType(CommandType.Group | CommandType.Friend)]
[Permission("vortex.admin.group")]
public static class GroupAdminCommand
{
    [Command("add", "添加")]
    [CommandType(CommandType.Group | CommandType.Friend)]
    [Permission("vortex.admin.group.add")]
    public static class AddCmd
    {
        [Main]
        public static async Task Execute(CommandArgs args, [Param("组名")] string groupName)
        {
            try 
            {
                Group.Add(groupName);
                await args.ReplyAsync($"组 {groupName} 添加成功!");
            }
            catch (Exception ex)
            {
                await args.ReplyAsync($"添加失败: {ex.Message}");
            }
        }
    }

    [Command("del", "删除")]
    [CommandType(CommandType.Group | CommandType.Friend)]
    [Permission("vortex.admin.group.del")]
    public static class DelCmd
    {
        [Main]
        public static async Task Execute(CommandArgs args, [Param("组名")] string groupName)
        {
            try
            {
                Group.Delete(groupName);
                await args.ReplyAsync($"组 {groupName} 删除成功!");
            }
            catch (Exception ex)
            {
                await args.ReplyAsync($"删除失败: {ex.Message}");
            }
        }
    }

    [Command("addperm", "添加权限")]
    [CommandType(CommandType.Group | CommandType.Friend)]
    [Permission("vortex.admin.group.addperm")]
    public static class AddPermCmd
    {
        [Main]
        public static async Task Execute(CommandArgs args, [Param("组名")] string groupName, [Param("权限")] string permission)
        {
            try
            {
                Group.AddPermission(groupName, permission);
                await args.ReplyAsync($"权限 {permission} 已添加到组 {groupName}");
            }
            catch (Exception ex)
            {
                await args.ReplyAsync($"添加权限失败: {ex.Message}");
            }
        }
    }

    [Command("delperm", "删除权限")]
    [CommandType(CommandType.Group | CommandType.Friend)]
    [Permission("vortex.admin.group.delperm")]
    public static class DelPermCmd
    {
        [Main]
        public static async Task Execute(CommandArgs args, [Param("组名")] string groupName, [Param("权限")] string permission)
        {
            try
            {
                Group.RemovePermission(groupName, permission);
                await args.ReplyAsync($"权限 {permission} 已从组 {groupName} 删除");
            }
            catch (Exception ex)
            {
                await args.ReplyAsync($"删除权限失败: {ex.Message}");
            }
        }
    }

    [Command("parent", "父组")]
    [CommandType(CommandType.Group | CommandType.Friend)]
    [Permission("vortex.admin.group.parent")]
    public static class ParentCmd
    {
        [Main]
        public static async Task Execute(CommandArgs args, [Param("组名")] string groupName, [Param("父组名")] string parentGroupName)
        {
            try
            {
                Group.SetParent(groupName, parentGroupName);
                await args.ReplyAsync($"组 {groupName} 的父组已更改为 {parentGroupName}");
            }
            catch (Exception ex)
            {
                await args.ReplyAsync($"设置父组失败: {ex.Message}");
            }
        }
    }

    [Command("list", "列表")]
    [CommandType(CommandType.Group | CommandType.Friend)]
    [Permission("vortex.admin.group.list")]
    public static class ListCmd
    {
        [Main]
        public static async Task Execute(CommandArgs args)
        {
            var groups = Group.GetAll();

            if (groups.Count == 0)
            {
                await args.ReplyAsync("还没有添加任何权限组!");
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("权限组列表:");
            foreach (var group in groups)
            {
                var perms = string.Join(", ", group.Permissions);
                var parent = string.IsNullOrEmpty(group.Parent?.Name) ? "无" : group.Parent?.Name;
                sb.AppendLine($"{group.Name} (父组: {parent})");
                sb.AppendLine($"  权限: {perms}");
            }

            await args.ReplyAsync(sb.ToString());
        }
    }
}
