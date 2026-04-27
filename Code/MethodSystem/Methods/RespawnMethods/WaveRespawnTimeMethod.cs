using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
public class WaveRespawnTimeMethod : SynchronousMethod
{
    public override string Description => "Changes the time left to spawn a wave.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new WaveTypeArgument("wave type"),
        new OptionsArgument("mode", "set", "add", "remove"),
        new DurationArgument("time")
    ];

    public override void Execute()
    {
        if (WaveTypeArgument.GetWave(Args.GetWaveType("wave type")) is not {} wave)
        {
            return;
        }

        var duration = Args.GetDuration("time");
        switch (Args.GetOption("mode"))
        {
            case "set": wave.TimeLeft = (float)duration.TotalSeconds; break;
            case "add": wave.TimeLeft += (float)duration.TotalSeconds; break;
            case "remove": wave.TimeLeft -= (float)duration.TotalSeconds; break;
        }
    }
}