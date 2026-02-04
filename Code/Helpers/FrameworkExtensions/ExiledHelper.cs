using LabApi.Loader;
using MEC;
using Logger = LabApi.Features.Console.Logger;

namespace SER.Code.Helpers.FrameworkExtensions;

public static class ExiledHelper
{
    private const string ExiledLoaderName = "Exiled Loader";

    public static event Action? OnExiledDetected;
    
    public static IEnumerator<float> Awaiter()
    {
        uint attempts = 0;
        while (PluginLoader.EnabledPlugins.All(p => p.Name != ExiledLoaderName))
        {
            if (attempts++ > 20)
            {
                Logger.Debug("SER <-> EXILED bind failed. EXILED specific methods will NOT be loaded.");
                yield break;
            }
                
            yield return Timing.WaitForSeconds(0.1f);
        }
        
        Logger.Raw("SER <-> EXILED bind was successful. EXILED specific methods will be loaded.", ConsoleColor.Green);
        OnExiledDetected?.Invoke();
    }
}