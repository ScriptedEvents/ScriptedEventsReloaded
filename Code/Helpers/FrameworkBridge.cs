using LabApi.Features.Console;
using LabApi.Loader;
using MEC;
using SER.Code.Extensions;
using SER.Code.MethodSystem;

namespace SER.Code.Helpers;

public class FrameworkBridge
{
    public record struct Framework(string Name, Type Type);
    public static readonly List<Framework> Found = [];
    private readonly List<CoroutineHandle> _handles = [];

    public enum Type
    {
        Exiled,
        Callvote,
        Ucr
    }
    
    private readonly Framework[] _frameworks =
    [
        new("Callvote", Type.Callvote),
#if !EXILED
        new("Exiled Loader", Type.Exiled),
#endif
        new("UncomplicatedCustomRoles", Type.Ucr)
    ];

    public void Load()
    {
        Timing.KillCoroutines(_handles.ToArray());
        _handles.Clear();
        Found.Clear();
#if EXILED
        MethodIndex.LoadMethodsOfFramework(Type.Exiled);
#endif
        Found.Clear();
        foreach (var framework in _frameworks)
        {
            _handles.Add(Timing.RunCoroutine(Await(framework)));
        }
    }

    public void Finish()
    {
        Timing.KillCoroutines(_handles.ToArray());
        _handles.Clear();
        Logger.Info(Found.Count == 0
            ? "No supported framework was found, no additional methods were added."
            : $"SER has added methods for {Found.Count} supported framework(s): " +
              $"{Found.Select(f => f.Type.ToString()).JoinStrings(", ")}"
        );
    }

    private static IEnumerator<float> Await(Framework framework)
    {
        // handled from forever repeating when coroutines are killed
        while (true)
        {
            yield return Timing.WaitForSeconds(.5f);
            
            if (IsLabAPIComatibleFrameworkLoaded(framework) || IsExiledCompatibleFrameworkLoaded(framework))
            {
                break;
            }
        }
        
        Logger.Debug($"SER found supported framework '{framework.Type}'");
        Found.Add(framework);
        MethodIndex.LoadMethodsOfFramework(framework.Type);
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
