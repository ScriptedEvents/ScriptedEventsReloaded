using LabApi.Features.Console;
using LabApi.Loader;
using MEC;
using SER.Code.Extensions;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.Helpers;

public class FrameworkBridge
{
    protected record struct Framework(string Name, IDependOnFramework.Type Type);

    private readonly List<Framework> _found = [];
    private readonly List<CoroutineHandle> _handles = [];

    private readonly Framework[] _frameworks =
    [
        new("Callvote", IDependOnFramework.Type.Callvote),
        new("Exiled Loader", IDependOnFramework.Type.Exiled),
        new("UncomplicatedCustomRoles", IDependOnFramework.Type.Ucr)
    ];

    public void Load()
    {
        foreach (var framework in _frameworks)
        {
            _handles.Add(Timing.RunCoroutine(Await(framework)));
        }
    }

    public string StopAndGetLoadedFrameworksMessage()
    {
        Timing.KillCoroutines(_handles.ToArray());
        _handles.Clear();
        if (_found.Count == 0) return "No supported framework was found, no additional methods were added.";
        return $"SER has added methods for {_found.Count} supported framework(s): " +
               $"{_found.Select(f => f.Type.ToString()).JoinStrings(", ")}";
    }

    private IEnumerator<float> Await(Framework framework)
    {
        for (int timer = 0; timer <= 3; timer++)
        {
            yield return Timing.WaitForSeconds(1f);

            if (_found.Contains(framework))
            {
                continue;
            }
            
            if (PluginLoader.EnabledPlugins.All(plg => plg.Name != framework.Name) && !IsExiledCompatFrameworkLoaded(framework))
            {
                continue;
            }

            _found.Add(framework);
            MethodIndex.LoadMethodsOfFramework(framework.Type);
        }

        Logger.Raw(StopAndGetLoadedFrameworksMessage(), ConsoleColor.DarkYellow);
    }

    private bool IsExiledCompatFrameworkLoaded(Framework framework)
    {
        if (framework.Type == IDependOnFramework.Type.Callvote) // As of right now, Callvote-Exiled is not compatible with SER.
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