using System.Reflection;
using Vortex.Bot.Attributes;

namespace Vortex.Bot.Command;

internal static class CommandTreeBuilder
{
    public static Command BuildTree(Type type, string name, string prefix)
    {
        bool skipHelp = type.GetCustomAttribute<SkipHelpAttribute>() != null;
        Command result = new Command(type, name, prefix, skipHelp);

        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            string[] aliases = AliasResolver.GetAliases(nestedType).ToArray();
            string displayName = aliases[0];
            var subCommand = BuildTree(nestedType, displayName, $"{prefix} {displayName}");
            foreach (string? alias in aliases)
                result.Add(alias, subCommand);
        }

        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            string[] aliases = AliasResolver.GetAliases(method).ToArray();
            bool isMain = method.GetCustomAttribute<MainAttribute>() != null;
            var flexibleAttr = method.GetCustomAttribute<FlexibleAttribute>();
            bool isFlexible = flexibleAttr != null;

            var executorType = isFlexible ? ExecutorType.Flexible : ExecutorType.Normal;
            int minArgs = isFlexible ? flexibleAttr!.MinArgs : 0;

            string displayName = aliases[0];
            string executorName = isMain ? "" : displayName;

            if (isMain)
            {
                CommandExecutor mainExecutor = new CommandExecutor(method, prefix, executorName, executorType, minArgs);
                result.Add(null, mainExecutor);
            }
            else
            {
                CommandExecutor executor = new CommandExecutor(method, prefix, executorName, executorType, minArgs);
                foreach (string? alias in aliases)
                    result.Add(alias, executor);
            }
        }

        result.PostBuildTree();
        return result;
    }
}
