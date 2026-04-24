using System.Reflection;

namespace Vortex.Bot;

internal static class Constants
{
    public const string Banner = """
    __     _____  ____ _____ _______  __
    \ \   / / _ \|  _ \_   _| ____\ \/ /
     \ \ / / | | | |_) || | |  _|  \  / 
      \ V /| |_| |  _ < | | | |___ /  \ 
       \_/  \___/|_| \_\|_| |_____/_/\_\
                Vortex.Bot
    """;

    public const string ConfigFileName = "appsettings.jsonc";
    public const string ConfigResourceName = $"Vortex.Bot.Resources.{ConfigFileName}";

    public static string ImplementationName = "Vortex.Bot";

    public static string ImplementationVersion = typeof(Constants).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion?[6..]
        ?? "Unknown";

    public static string Version = "1.0";
}