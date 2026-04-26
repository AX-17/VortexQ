namespace Vortex.Bot.Command;

internal readonly record struct ParseResult(CommandBase Current, int Unmatched);
