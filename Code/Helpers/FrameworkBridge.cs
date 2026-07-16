using LabApi.Features.Console;
using LabApi.Loader;
using SER.Code.Extensions;
using SER.Code.MethodSystem;

namespace SER.Code.Helpers;

public static class FrameworkBridge
{
    public record struct Framework(string Name, Type Type);
    public static readonly List<Framework> Found = new(4);

    public enum Type
    {
        Exiled,
        Callvote,
        UncomplicatedCustomRoles,
        ProjectMapEditorReborn
    }

    private static readonly Framework[] Frameworks =
    [
#if !EXILED
        new("Exiled Loader", Type.Exiled),
#endif
        new("Callvote", Type.Callvote),
        new("ProjectMER", Type.ProjectMapEditorReborn),
        new("UncomplicatedCustomRoles", Type.UncomplicatedCustomRoles)
    ];

    public static void Initialize()
    {
        Clear();
        FindAndLoadFrameworkMethods();
    }

    public static void Clear()
    {
        Found.Clear();
    }

    public static void FindAndLoadFrameworkMethods()
    {
        foreach (var framework in Frameworks.Except(Found))
        {
            if (IsLabAPIComatibleFrameworkLoaded(framework) || IsExiledCompatibleFrameworkLoaded(framework))
            {
                MethodIndex.LoadMethodsOfFramework(framework.Type);
                Found.Add(framework);
                Logger.Debug($"Found {framework.Name} plugin, adding methods...");
            }
        }
    }

    public static void PrintFound()
    {
        Logger.Debug(Found.Count == 0
            ? "No supported framework was found, no additional methods were added."
            : $"SER has added methods for {Found.Count} supported framework(s): " +
              $"{Found.Select(f => f.Type.ToString()).JoinStrings(", ")}"
        );
    }

    private static bool IsLabAPIComatibleFrameworkLoaded(Framework framework)
    {
        return PluginLoader.EnabledPlugins.Any(plg => plg.Name == framework.Name);
    }

    private static bool IsExiledCompatibleFrameworkLoaded(Framework framework)
    {
        // As of right now, Callvote-Exiled is not compatible with SER.
        if (framework.Type == FrameworkBridge.Type.Callvote) 
        {
            return false;
        }

        if (PluginLoader.Plugins.All(plg => plg.Key.Name != "Exiled Loader"))
        {
            return false;
        }
        
        return Exiled.Loader.Loader.Plugins.Any(plg => plg.Name == framework.Name);
    }
}
