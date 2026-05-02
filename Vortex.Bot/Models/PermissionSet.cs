namespace Vortex.Bot.Models;

public sealed class PermissionSet
{
    private readonly HashSet<string> _permissions = [];
    private readonly HashSet<string> _negatedPermissions = [];

    public IReadOnlySet<string> Permissions => _permissions;
    public IReadOnlySet<string> NegatedPermissions => _negatedPermissions;

    public void Add(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return;

        if (permission.StartsWith('!'))
        {
            var actualPerm = permission[1..];
            if (_negatedPermissions.Add(actualPerm))
            {
                _permissions.Remove(actualPerm);
            }
        }
        else
        {
            if (_permissions.Add(permission))
            {
                _negatedPermissions.Remove(permission);
            }
        }
    }

    public void Remove(string permission)
    {
        if (permission.StartsWith('!'))
        {
            _negatedPermissions.Remove(permission[1..]);
        }
        else
        {
            _permissions.Remove(permission);
        }
    }

    public void Clear()
    {
        _permissions.Clear();
        _negatedPermissions.Clear();
    }

    public void Set(IEnumerable<string> permissions)
    {
        Clear();
        foreach (var perm in permissions)
        {
            Add(perm);
        }
    }

    public bool Contains(string permission)
    {
        if (string.IsNullOrEmpty(permission))
            return true;

        if (_negatedPermissions.Contains(permission))
            return false;

        if (_permissions.Contains(permission))
            return true;

        return MatchesWildcard(permission);
    }

    private bool MatchesWildcard(string permission)
    {
        var nodes = permission.Split('.');
        for (var i = nodes.Length - 1; i >= 0; i--)
        {
            nodes[i] = "*";
            var wildcardPerm = string.Join(".", nodes, 0, i + 1);

            if (_negatedPermissions.Contains(wildcardPerm))
                return false;

            if (_permissions.Contains(wildcardPerm))
                return true;
        }

        return false;
    }

    public IEnumerable<string> ToEnumerable()
    {
        foreach (var perm in _permissions)
            yield return perm;

        foreach (var perm in _negatedPermissions)
            yield return $"!{perm}";
    }

    public override string ToString() => string.Join(",", ToEnumerable());

    public static PermissionSet Parse(string? permissionString)
    {
        var set = new PermissionSet();

        if (string.IsNullOrWhiteSpace(permissionString))
            return set;

        foreach (var perm in permissionString.Split(','))
        {
            set.Add(perm.Trim());
        }

        return set;
    }
}
