using Vortex.Bot.Command;

namespace Vortex.Bot.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute(params string[] aliases) : Attribute
{
    public HashSet<string> Alias { get; } = [.. aliases];
}

[AttributeUsage(AttributeTargets.Class)]
public class CommandTypeAttribute(CommandType commandType) : Attribute
{
    public CommandType CommandType { get; } = commandType;
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PermissionAttribute(params string[] permissions) : Attribute
{
    public string[] Permissions { get; } = permissions;
}

[AttributeUsage(AttributeTargets.Method)]
public class MainAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public class FlexibleAttribute : Attribute
{
    /// <summary>
    /// 最小参数数量（可选参数之前的必需参数数量）
    /// </summary>
    public int MinArgs { get; }

    /// <summary>
    /// 允许参数数量可变
    /// </summary>
    public FlexibleAttribute()
    {
        MinArgs = 0;
    }

    /// <summary>
    /// 允许参数数量可变，指定最小参数数量
    /// </summary>
    /// <param name="minArgs">最小参数数量（可选参数之前的必需参数数量）</param>
    public FlexibleAttribute(int minArgs)
    {
        MinArgs = minArgs;
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AliasAttribute(params string[] aliases) : Attribute
{
    public HashSet<string> Alias { get; } = [.. aliases];
}

[AttributeUsage(AttributeTargets.Parameter)]
public class ParamAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}
