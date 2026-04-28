using Respawning;
using Respawning.Waves;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
public class PlayWaveEffectMethod : SynchronousMethod
{
    public override string Description => "Plays a Respawn Wave effect (the NTF helicopter/CI van arrival animation)";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("wave faction",
            "ntf",
            "ci")
    ];
    public override void Execute()
    {
        var faction = Args.GetOption("wave faction");
        
        WaveUpdateMessage.ServerSendUpdate(
            WaveManager.Waves.First(wave => 
                faction == "ntf"
                    ? wave is NtfSpawnWave
                    : wave is ChaosSpawnWave),
            UpdateMessageFlags.Trigger);
    }
}