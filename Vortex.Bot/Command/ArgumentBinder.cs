using System.Reflection;
using System.Text;
using Vortex.Bot.Attributes;

namespace Vortex.Bot.Command;

internal sealed class ArgumentBinder
{
    private readonly ArgumentParser[] _parsers;
    private readonly Type[] _types;
    private readonly string _parameterInfo;

    public ArgumentBinder(ParameterInfo[] parameters)
    {
        ParameterInfo[] commandParameters = parameters.Skip(1).ToArray();
        _parsers = CreateParsers(commandParameters);
        _types = CreateTypes(commandParameters);
        _parameterInfo = BuildParameterInfo(commandParameters);
    }

    public string ParameterInfo => _parameterInfo;

    public object?[]? ParseArguments(List<string> parameters, int startIndex)
    {
        int count = _parsers.Length;
        object?[] args = new object?[count + 1];
        args[0] = null;

        for (int i = 0; i < count; i++)
        {
            int paramIndex = startIndex + i;

            if (paramIndex >= parameters.Count)
            {
                args[i + 1] = GetDefaultValue(_types[i]);
                continue;
            }

            if (!_parsers[i](parameters[paramIndex], out args[i + 1]))
                return null;
        }

        return args;
    }

    public ValidationResult GetValidationRules(int startIndex, ExecutorType executorType, int minArgs)
    {
        if (executorType == ExecutorType.Flexible)
        {
            return new ValidationResult(minArgs + startIndex, int.MaxValue);
        }

        int expectedCount = _parsers.Length + startIndex;
        return new ValidationResult(expectedCount, expectedCount);
    }

    private static ArgumentParser[] CreateParsers(ParameterInfo[] parameters)
    {
        return [.. parameters.Select(p =>
        {
            return !CommandParser.IsSupportedType(p.ParameterType)
                ? throw new NotSupportedException(
                    $"Parameter type {p.ParameterType.Name} is not supported")
                : CommandParser.GetParser(p.ParameterType); })];
    }

    private static Type[] CreateTypes(ParameterInfo[] parameters)
    {
        return [.. parameters.Select(p => p.ParameterType)];
    }

    private static string BuildParameterInfo(ParameterInfo[] parameters)
    {
        if (parameters.Length == 0)
            return "";

        var sb = new StringBuilder();
        foreach (ParameterInfo p in parameters)
        {
            ParamAttribute? paramAttr = p.GetCustomAttribute<ParamAttribute>();
            string paramDesc = paramAttr?.Description ?? p.Name ?? "param";
            sb.Append($" <{paramDesc}: {CommandParser.GetFriendlyName(p.ParameterType)}>");
        }
        return sb.ToString();
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
        return type == typeof(bool) ? false : type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
