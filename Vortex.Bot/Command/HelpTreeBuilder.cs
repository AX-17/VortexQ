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

        // 首先显示命令本身的 Main 方法
        var mainExecutor = command.GetMainCommands()
            .OfType<CommandExecutor>()
            .FirstOrDefault();
        
        if (mainExecutor != null)
        {
            var helpText = command.HelpText ?? mainExecutor.HelpText;
            var node = new HelpNode(
                rootPath,
                mainExecutor.ParameterInfo,
                helpText
            );
            BuildCommandLine(node, "", true);
        }

        // 然后显示子命令
        var children = GetVisibleChildren(command, rootPath);
        
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var isLast = i == children.Count - 1;
            BuildCommandLine(child, "", isLast);
        }
        
        return _sb.ToString().TrimEnd();
    }

    private void BuildCommandLine(HelpNode node, string indent, bool isLast)
    {
        var branch = isLast ? "└── " : "├── ";
        
        // 构建命令行：路径 + 参数
        var line = $"{indent}{branch}{node.FullPath}{node.ParamInfo}";
        
        // 如果有帮助文本，添加到同一行
        if (!string.IsNullOrEmpty(node.HelpText))
        {
            line += $" - {node.HelpText}";
        }
        
        _sb.AppendLine(line);
        
        if (node.SubCommands.Count > 0)
        {
            var childIndent = indent + (isLast ? "    " : "│   ");
            
            for (int i = 0; i < node.SubCommands.Count; i++)
            {
                var child = node.SubCommands[i];
                var isLastChild = i == node.SubCommands.Count - 1;
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
            var firstName = item.Names.First();
            var fullPath = string.IsNullOrEmpty(parentPath) ? firstName : $"{parentPath} {firstName}";
            
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
                var mainExecutor = subCmd.GetMainCommands()
                    .OfType<CommandExecutor>()
                    .FirstOrDefault();
                var paramInfo = mainExecutor?.ParameterInfo ?? "";
                var helpText = subCmd.HelpText ?? mainExecutor?.HelpText;
                
                var subChildren = GetVisibleChildren(subCmd, fullPath);
                
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

internal sealed class HelpNode
{
    public string FullPath { get; }
    public string ParamInfo { get; }
    public string? HelpText { get; }
    public List<HelpNode> SubCommands { get; }

    public HelpNode(string fullPath, string paramInfo, string? helpText = null, List<HelpNode>? subCommands = null)
    {
        FullPath = fullPath;
        ParamInfo = paramInfo;
        HelpText = helpText;
        SubCommands = subCommands ?? new List<HelpNode>();
    }
}
