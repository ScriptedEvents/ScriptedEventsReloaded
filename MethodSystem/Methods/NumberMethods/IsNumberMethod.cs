using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.NumberMethods;

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