using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.NumberMethods;

[UsedImplicitly]
public class IsNumberMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns true if the provided value is a number.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new AnyValueArgument("value to check")
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetAnyValue("value to check") is NumberValue;
    }
}