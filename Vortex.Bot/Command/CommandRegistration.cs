namespace Vortex.Bot.Command;

internal sealed class CommandRegistration(Command tree, CommandType types, string[] aliases)
{
    public Command Tree { get; } = tree;
    public CommandType Types { get; set; } = types;
    public string[] Aliases { get; } = aliases;
}
