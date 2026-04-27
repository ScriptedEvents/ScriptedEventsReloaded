using JetBrains.Annotations;
using Respawning;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
public class SpawnWaveMethod : SynchronousMethod
{
    public override string Description => "Forces a wave to start.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new WaveTypeArgument("wave type"),
        new OptionsArgument("mode", "withAnimation", "instant")
        {
            DefaultValue = new("withAnimation", null)
        }
    ];
    
    public override void Execute()
    {
        if (WaveTypeArgument.GetWave(Args.GetWaveType("wave type")) is not {} wave)
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