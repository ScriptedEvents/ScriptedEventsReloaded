using LabApi.Features.Console;
using LabApi.Loader;
using MEC;
using SER.Code.Extensions;

namespace SER.Code.Helpers.FrameworkExtensions;

public static class CallVoteHelper
{
    private static readonly string Name = "Callvote";

    public static event Action? OnCallvoteDetected;
    
    public static IEnumerator<float> Awaiter()
    {
        uint attempts = 0;
        while (PluginLoader.EnabledPlugins.All(plg => plg.Name != Name))
        {
            if (attempts++ > 20)
            {
                Logger.Debug($"SER <-> Callvote bind failed. Callvote specific methods will NOT be loaded. Enabled plugins: {PluginLoader.EnabledPlugins.Select(p => p.Name).JoinStrings(", ")}");
                yield break;
            }
                
            yield return Timing.WaitForSeconds(0.1f);
        }
        
        Logger.Raw("SER <-> Callvote bind was successful. Callvote specific methods will be loaded.", ConsoleColor.Green);
        OnCallvoteDetected?.Invoke();
    }
}