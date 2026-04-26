using System.Text;

namespace Vortex.Bot.Command;

internal sealed class HelpTreeBuilder(string rootPath)
{
    private readonly StringBuilder _sb = new();
    private readonly string _rootPath = rootPath;

    public string Build(Command command)
    {
        _sb.Clear();
        _sb.AppendLine("📋 可用指令:");
        _sb.AppendLine();

        CommandExecutor? mainExecutor = command.GetMainCommands()
            .OfType<CommandExecutor>()
            .FirstOrDefault();

        if (mainExecutor != null)
        {
            string? helpText = command.HelpText ?? mainExecutor.HelpText;
            var node = new HelpNode(
                _rootPath,
                mainExecutor.ParameterInfo,
                helpText
            );
            BuildCommandLine(node, "", true);
        }

        List<HelpNode> children = GetVisibleChildren(command, _rootPath);

        for (int i = 0; i < children.Count; i++)
        {
            HelpNode child = children[i];
            bool isLast = i == children.Count - 1;
            BuildCommandLine(child, "", isLast);
        }

        return _sb.ToString().TrimEnd();
    }

    private void BuildCommandLine(HelpNode node, string indent, bool isLast)
    {
        string branch = isLast ? "└── " : "├── ";

        string line = $"{indent}{branch}{node.FullPath}{node.ParamInfo}";

        if (!string.IsNullOrEmpty(node.HelpText))
        {
            line += $" - {node.HelpText}";
        }

        _sb.AppendLine(line);

        if (node.SubCommands.Count > 0)
        {
            string childIndent = indent + (isLast ? "    " : "│   ");

            for (int i = 0; i < node.SubCommands.Count; i++)
            {
                HelpNode child = node.SubCommands[i];
                bool isLastChild = i == node.SubCommands.Count - 1;
                BuildCommandLine(child, childIndent, isLastChild);
            }
        }
    }

    private List<HelpNode> GetVisibleChildren(Command command, string parentPath)
    {
        var nodes = new List<HelpNode>();

        var grouped = command.GetNamedCommands()
            .SelectMany(kv => kv.Value.Select(cmd => new { Name = kv.Key, Command = cmd }))
            .Where(item => item.Command.GetType().Name != "HelpCommand")
            .GroupBy(item => item.Command)
            .Select(g => new
            {
                Command = g.Key,
                Names = g.Select(x => x.Name).Distinct().ToList()
            });

        foreach (var item in grouped)
        {
            string firstName = item.Names.First();
            string fullPath = string.IsNullOrEmpty(parentPath) ? firstName : $"{parentPath} {firstName}";

            if (item.Command is CommandExecutor executor)
            {
                nodes.Add(new HelpNode(
                    fullPath,
                    executor.ParameterInfo,
                    executor.HelpText
                ));
            }
            else if (item.Command is Command subCmd)
            {
                CommandExecutor? mainExecutor = subCmd.GetMainCommands()
                    .OfType<CommandExecutor>()
                    .FirstOrDefault();
                string paramInfo = mainExecutor?.ParameterInfo ?? "";
                string? helpText = subCmd.HelpText ?? mainExecutor?.HelpText;

                List<HelpNode> subChildren = GetVisibleChildren(subCmd, fullPath);

                nodes.Add(new HelpNode(
                    fullPath,
                    paramInfo,
                    helpText,
                    subChildren
                ));
            }
        }

        return nodes;
    }
}

internal sealed class HelpNode(string fullPath, string paramInfo, string? helpText = null, List<HelpNode>? subCommands = null)
{
    public string FullPath { get; } = fullPath;
    public string ParamInfo { get; } = paramInfo;
    public string? HelpText { get; } = helpText;
    public List<HelpNode> SubCommands { get; } = subCommands ?? [];
}
