using Vortex.Bot.Command;

namespace Vortex.Bot.Extension;

internal static class CommandTypeExtensions
{
    public static bool Supports(this CommandType available, CommandType required)
    {
        return available.HasFlag(required);
    }
}
