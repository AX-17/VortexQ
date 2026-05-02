using Vortex.Bot.Database.Models;

namespace Vortex.Bot.Models;

public sealed class PermissionInheritanceResolver(Func<string, Group?> groupResolver)
{
    private readonly Func<string, Group?> _groupResolver = groupResolver;

    public PermissionSet ResolveEffectivePermissions(Group group)
    {
        var effectivePerms = new PermissionSet();
        var negatedPerms = new HashSet<string>();
        var traversed = new HashSet<string>();

        var current = group;
        while (current != null)
        {
            if (!traversed.Add(current.Name))
            {
                throw new InvalidOperationException($"检测到循环继承: {current.Name}");
            }

            foreach (var perm in current.LocalPermissions.Permissions)
            {
                if (!negatedPerms.Contains(perm))
                {
                    effectivePerms.Add(perm);
                }
            }

            foreach (var perm in current.LocalPermissions.NegatedPermissions)
            {
                effectivePerms.Remove(perm);
                negatedPerms.Add(perm);
            }

            current = ResolveParent(current);
        }

        return effectivePerms;
    }

    public bool HasPermission(Group group, string permission)
    {
        if (string.IsNullOrEmpty(permission))
            return true;

        var traversed = new HashSet<string>();
        var current = group;

        while (current != null)
        {
            if (!traversed.Add(current.Name))
            {
                throw new InvalidOperationException($"检测到循环继承: {current.Name}");
            }

            if (current.LocalPermissions.NegatedPermissions.Contains(permission))
                return false;

            if (current.LocalPermissions.Permissions.Contains(permission))
                return true;

            if (MatchesWildcard(current, permission, out bool negated))
            {
                return !negated;
            }

            current = ResolveParent(current);
        }

        return false;
    }

    private static bool MatchesWildcard(Group group, string permission, out bool negated)
    {
        negated = false;
        var nodes = permission.Split('.');

        for (var i = nodes.Length - 1; i >= 0; i--)
        {
            nodes[i] = "*";
            var wildcardPerm = string.Join(".", nodes, 0, i + 1);

            if (group.LocalPermissions.NegatedPermissions.Contains(wildcardPerm))
            {
                negated = true;
                return true;
            }

            if (group.LocalPermissions.Permissions.Contains(wildcardPerm))
            {
                negated = false;
                return true;
            }
        }

        return false;
    }

    private Group? ResolveParent(Group group)
    {
        if (string.IsNullOrEmpty(group.ParentName))
            return null;

        return _groupResolver(group.ParentName);
    }
}
