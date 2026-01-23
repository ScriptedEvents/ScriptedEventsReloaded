using JetBrains.Annotations;
using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Yielding;

namespace SER.Code.MethodSystem.Methods.WaitingMethods;

[UsedImplicitly]
public class WaitMethod : YieldingMethod
{
    public override string Description => "Halts execution of the script for a specified amount of time.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DurationArgument("duration")
    ];

    public override IEnumerator<float> Execute()
    {
        var dur = Args.GetDuration("duration");
        yield return Timing.WaitForSeconds((float)dur.TotalSeconds);
    }
}