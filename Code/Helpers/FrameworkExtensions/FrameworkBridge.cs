using LabApi.Features.Console;
using LabApi.Loader;
using MEC;
using PluginSCPSL.Library.Utility;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.Helpers.FrameworkExtensions;

public abstract class FrameworkBridge : Registerable<FrameworkBridge>
{
    protected abstract string Name { get; }
    public abstract IDependOnFramework.Type FrameworkType { get; }
    public event Action? OnDetected;

    public override bool IsDebug { get; } = true;

    protected override void OnRegistered()
    {
        OnDetected += () => MethodIndex.LoadMethodsOfFramework(FrameworkType);
        Await(OnDetected).RunCoroutine();
        base.OnRegistered();
    }

    protected IEnumerator<float> Await(Action? onDetected)
    {
        uint attempts = 0;
        while (PluginLoader.EnabledPlugins.All(plg => plg.Name != Name))
        {
            if (attempts++ > 20)
            {
                Logger.Debug($"SER <-> {Name} bind failed. {Name} specific methods will NOT be loaded.");
                yield break;
            }
            
            yield return Timing.WaitForSeconds(0.1f);
        }
    
        Logger.Raw($"SER <-> {Name} bind was successful! {Name} specific methods will be loaded.", ConsoleColor.Green);
        onDetected?.Invoke();
    }
}