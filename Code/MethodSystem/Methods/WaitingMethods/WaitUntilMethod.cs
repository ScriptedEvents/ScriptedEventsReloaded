using JetBrains.Annotations;
using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Yielding;

namespace SER.Code.MethodSystem.Methods.WaitingMethods;

[UsedImplicitly]
public class WaitUntilMethod : YieldingMethod
{
    public override string Description => "Halts execution of the script until the given condition is true.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new BoolArgument("condition")
        {
            IsFunction = true
        }
    ];

    public override IEnumerator<float> Execute()
    {
        var condFunc = Args.GetBoolFunc("condition");
        while (!condFunc()) yield return Timing.WaitForOneFrame;
    }
}