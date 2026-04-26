using System.Reflection;
using System.Text;
using Vortex.Bot.Attributes;

namespace Vortex.Bot.Command;

internal sealed class CommandExecutor : CommandBase
{
    private readonly CommandParser.Parser[] _argParsers;
    private readonly Type[] _argTypes;
    private readonly MethodInfo _method;
    private readonly bool _isFlexible;
    private readonly int _minArgs;
    private readonly Type _argsType;
    private readonly string _parentPrefix;
    private readonly string _commandName;
    private readonly string _paramInfo;

    public CommandExecutor(
        MethodInfo method,
        string parentPrefix,
        string commandName,
        bool isFlexible = false) : base(method)
    {
        _method = method;
        _isFlexible = isFlexible;
        _parentPrefix = parentPrefix;
        _commandName = commandName;

        var parameters = method.GetParameters();
        ValidateParameters(parameters);

        var flexibleAttr = method.GetCustomAttribute<FlexibleAttribute>();
        _minArgs = flexibleAttr?.MinArgs ?? 0;

        _argsType = parameters[0].ParameterType;
        _argParsers = CreateParsers(parameters);
        _argTypes = CreateArgTypes(parameters);
        _paramInfo = BuildParamInfo(parameters);

        UpdateInfo(commandName);
    }

    private void ValidateParameters(ParameterInfo[] parameters)
    {
        if (parameters.Length == 0 || !typeof(CommandArgs).IsAssignableFrom(parameters[0].ParameterType))
        {
            throw new InvalidOperationException(
                $"Method {_method.Name} must have a CommandArgs parameter as the first argument");
        }
    }

    private CommandParser.Parser[] CreateParsers(ParameterInfo[] parameters)
    {
        return parameters
            .Skip(1)
            .Select(p =>
            {
                if (!CommandParser.IsSupportedType(p.ParameterType))
                {
                    throw new NotSupportedException(
                        $"Parameter type {p.ParameterType.Name} is not supported for command method {_method.Name}");
                }
                return CommandParser.GetParser(p.ParameterType);
            })
            .ToArray();
    }

    private Type[] CreateArgTypes(ParameterInfo[] parameters)
    {
        return parameters
            .Skip(1)
            .Select(p => p.ParameterType)
            .ToArray();
    }

    private string BuildParamInfo(ParameterInfo[] parameters)
    {
        var sb = new StringBuilder();
        foreach (var p in parameters.Skip(1))
        {
            var paramAttr = p.GetCustomAttribute<ParamAttribute>();
            var paramDesc = paramAttr?.Description ?? p.Name ?? "param";
            sb.Append($" <{paramDesc}: {CommandParser.GetFriendlyName(p.ParameterType)}>");
        }
        return sb.ToString();
    }

    public void UpdateInfo(string actualCommandName)
    {
        if (string.IsNullOrEmpty(actualCommandName))
        {
            Info = $"{_parentPrefix}{_paramInfo}";
        }
        else
        {
            Info = $"{_parentPrefix} {actualCommandName}{_paramInfo}";
        }
    }

    public string GetParamInfo() => _paramInfo;

    public bool SupportsArgsType(Type argsType) => _argsType.IsAssignableFrom(argsType);

    public override async Task<ParseResult> TryParseAsync(CommandArgs args, int current, string commandName)
    {
        if (current > 0)
        {
            var actualCmd = args.Params[current - 1];
            UpdateInfo(actualCmd);
        }

        if (!SupportsArgsType(args.GetType()))
            return GetResult(int.MaxValue);

        return await ExecuteAsync(args, current);
    }

    private async Task<ParseResult> ExecuteAsync(CommandArgs args, int current)
    {
        var p = args.Params;
        var n = _argParsers.Length;
        var expectedCount = n + current;

        if (_isFlexible)
        {
            var minExpectedCount = _minArgs + current;
            if (p.Count < minExpectedCount)
                return GetResult(minExpectedCount - p.Count);

            if (p.Count > expectedCount)
                return GetResult(p.Count - expectedCount);
        }
        else
        {
            if (p.Count != expectedCount)
                return GetResult(Math.Abs(expectedCount - p.Count));
        }

        var invokeArgs = new object?[n + 1];
        invokeArgs[0] = args;

        for (int i = 0; i < n; i++)
        {
            var paramIndex = current + i;
            if (paramIndex >= p.Count)
            {
                if (_isFlexible && i >= _minArgs)
                {
                    invokeArgs[i + 1] = GetDefaultValue(_argTypes[i]);
                    continue;
                }
                return GetResult(n - i);
            }

            if (!_argParsers[i](p[paramIndex], out invokeArgs[i + 1]))
                return GetResult(n - i);
        }

        var permResult = await CheckPermissionAsync(args);
        if (permResult.Result != PermissionResult.Granted)
        {
            await args.ReplyAsync(permResult.DenyMessage ?? "你没有权限执行此指令。");
            return GetResult(0);
        }
        var result = _method.Invoke(null, invokeArgs);
        if (result is Task task)
            await task;

        return GetResult(0);
    }

    private static object? GetDefaultValue(Type type)
    {
        if (type == typeof(string))
            return string.Empty;
        if (type == typeof(int))
            return 0;
        if (type == typeof(long))
            return 0L;
        if (type == typeof(double))
            return 0.0;
        if (type == typeof(bool))
            return false;
        if (type.IsValueType)
            return Activator.CreateInstance(type);
        return null;
    }
}
