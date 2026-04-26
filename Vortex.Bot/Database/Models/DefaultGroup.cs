namespace Vortex.Bot.Database.Models;

public class DefaultGroup : Group
{
    public static readonly List<string> DefaultPermissions =
    [
        "user.help",
        "user.info",
        "user.register",
        "user.status",
        "user.ping",
        "user.time",
        "vortex.help",
        "vortex.selfinfo",
        "vortex.sign",
        "vortex.user",
        "vortex.user.register",
        "vortex.user.info"
    ];

    public const string DefaultGroupName = "default";

    public static readonly DefaultGroup Instance = new();

    private DefaultGroup() : base()
    {
        Name = DefaultGroupName;
        SetPermissions(DefaultPermissions);
    }

    public static void Initialize()
    {
        if (!Group.Exists(DefaultGroupName))
        {
            try
            {
                Group.Add(DefaultGroupName, string.Join(",", DefaultPermissions), "");
            }
            catch
            {
            }
        }
    }
}
