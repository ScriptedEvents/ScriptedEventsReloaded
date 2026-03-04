using LabApi.Features.Console;
using LabApi.Loader;
using MEC;
using SER.Code.Extensions;
using SER.Code.MethodSystem;

namespace SER.Code.Helpers;

public class FrameworkBridge
{
    protected record struct Framework(string Name, Type Type);

    private readonly List<Framework> _found = [];
    private readonly List<CoroutineHandle> _handles = [];

    public enum Type
    {
        None,
        Exiled,
        Callvote,
        Ucr
    }
    
    private readonly Framework[] _frameworks =
    [
        new("Callvote", Type.Callvote),
        new("Exiled Loader", Type.Exiled),
        new("UncomplicatedCustomRoles", Type.Ucr)
    ];

    public void Load()
    {
        foreach (var framework in _frameworks)
        {
            _handles.Add(Timing.RunCoroutine(Await(framework)));
        }

        Timing.CallDelayed(3f, () =>
        {
            Timing.KillCoroutines(_handles.ToArray());
            _handles.Clear();
            Logger.Info(_found.Count == 0
                ? "No supported framework was found, no additional methods were added."
                : $"SER has added methods for {_found.Count} supported framework(s): " +
                  $"{_found.Select(f => f.Type.ToString()).JoinStrings(", ")}"
            );
        });
    }

    private IEnumerator<float> Await(Framework framework)
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
        _found.Add(framework);
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

        if (PluginLoader.Plugins.Any(plg => plg.Key.Name == "Exiled Loader"))
        {
            return Exiled.Loader.Loader.Plugins.Any(plg => plg.Name == framework.Name);
        }

        return false;
    }
}