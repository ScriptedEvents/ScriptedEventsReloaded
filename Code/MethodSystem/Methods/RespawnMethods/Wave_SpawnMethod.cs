using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Wave_SpawnMethod : SynchronousMethod
{
    public override string Description => "Forces a wave to start.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new WaveArgument("wave type"),
        new OptionsArgument("mode", "withAnimation", "instant")
        {
            DefaultValue = new("withAnimation", null)
        }
    ];
    
    public override void Execute()
    {
        if (Args.GetWave("wave type") is not {} wave)
        {
            return;
        }

        switch (Args.GetOption("mode"))
        {
            case "withanimation": wave.InitiateRespawn(); break;
            case "instant": wave.InstantRespawn(); break;
        }
    }
}