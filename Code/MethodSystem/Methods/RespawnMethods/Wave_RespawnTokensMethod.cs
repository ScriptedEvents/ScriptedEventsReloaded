using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Wave_RespawnTokensMethod : SynchronousMethod
{
    public override string Description => "Changes respawn tokens of a wave.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new WaveArgument("wave type"),
        new OptionsArgument("mode", "set", "changeBy"),
        new IntArgument("respawn tokens")
    ];

    public override void Execute()
    {
        if (Args.GetWave("wave type") is not {} wave)
        {
            return;
        }
        
        if (Args.GetOption("mode") is "set")
        {
            wave.RespawnTokens = Args.GetInt("respawn tokens");
        }
        else
        {
            wave.RespawnTokens += Args.GetInt("respawn tokens");
        }
    }
}