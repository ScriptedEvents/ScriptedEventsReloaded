using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
public class WaveInfluenceMethod : SynchronousMethod
{
    public override string Description => "Changes influence of a wave.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new WaveTypeArgument("wave type"),
        new OptionsArgument("mode", "set", "changeBy"),
        new FloatArgument("influence")
    ];

    public override void Execute()
    {
        if (WaveTypeArgument.GetWave(Args.GetWaveType("wave type")) is not {} wave)
        {
            return;
        }
        
        if (Args.GetOption("mode") is "set")
        {
            wave.Influence = Args.GetFloat("influence");
        }
        else
        {
            wave.Influence += Args.GetFloat("influence");
        }
    }
}