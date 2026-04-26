using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Vortex.Bot.Command;

internal delegate bool ArgumentParser(string arg, [MaybeNullWhen(false)] out object obj);

internal static class CommandParser
{
    private static readonly Dictionary<Type, ArgumentParser> Parsers = new()
    {
        [typeof(bool)] = TryParseBool,
        [typeof(uint)] = TryParseUint,
        [typeof(int)] = TryParseInt,
        [typeof(long)] = TryParseLong,
        [typeof(ulong)] = TryParseUlong,
        [typeof(string)] = TryParseString,
        [typeof(DateTime)] = TryParseDateTime,
        [typeof(double)] = TryParseDouble,
        [typeof(float)] = TryParseFloat,
    };

    private static readonly Dictionary<Type, string> FriendlyNames = new()
    {
        [typeof(bool)] = "bool",
        [typeof(uint)] = "uint",
        [typeof(int)] = "int",
        [typeof(long)] = "long",
        [typeof(ulong)] = "ulong",
        [typeof(string)] = "str",
        [typeof(DateTime)] = "date",
        [typeof(double)] = "double",
        [typeof(float)] = "float",
    };

    public static ArgumentParser GetParser(Type type)
    {
        return Parsers.TryGetValue(type, out ArgumentParser? parser)
            ? parser
            : throw new NotSupportedException($"Type {type.Name} is not supported as a command parameter");
    }

    public static string GetFriendlyName(Type type)
    {
        return FriendlyNames.TryGetValue(type, out string? name) ? name : type.Name.ToLower();
    }

    public static bool IsSupportedType(Type type)
    {
        return Parsers.ContainsKey(type);
    }

    private static bool TryParseBool(string arg, out object obj)
    {
        bool result = bool.TryParse(arg, out bool t);
        obj = t;
        return result;
    }

    private static bool TryParseUint(string arg, out object obj)
    {
        bool result = uint.TryParse(arg, out uint t);
        obj = t;
        return result;
    }

    private static bool TryParseInt(string arg, out object obj)
    {
        bool result = int.TryParse(arg, out int t);
        obj = t;
        return result;
    }

    private static bool TryParseLong(string arg, out object obj)
    {
        bool result = long.TryParse(arg, out long t);
        obj = t;
        return result;
    }

    private static bool TryParseString(string arg, out object obj)
    {
        obj = arg;
        return true;
    }

    private static bool TryParseDateTime(string arg, out object obj)
    {
        bool result = DateTime.TryParse(arg, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime t);
        obj = t;
        return result;
    }

    private static bool TryParseDouble(string arg, out object obj)
    {
        bool result = double.TryParse(arg, CultureInfo.InvariantCulture, out double t);
        obj = t;
        return result;
    }

    private static bool TryParseFloat(string arg, out object obj)
    {
        bool result = float.TryParse(arg, CultureInfo.InvariantCulture, out float t);
        obj = t;
        return result;
    }

    private static bool TryParseUlong(string arg, out object obj)
    {
        bool result = ulong.TryParse(arg, out ulong t);
        obj = t;
        return result;
    }
}
