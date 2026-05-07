using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Wave_InfluenceMethod : SynchronousMethod
{
    public override string Description => "Changes influence of a wave.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("mode", "set", "change"),
        new WaveArgument("wave type"),
        new FloatArgument("influence")
    ];

    public override void Execute()
    {
        if (Args.GetWave("wave type") is not {} wave)
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