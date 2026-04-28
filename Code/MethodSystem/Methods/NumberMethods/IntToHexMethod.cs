using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.NumberMethods;

[UsedImplicitly]
public class IntToHexMethod : ReturningMethod<TextValue>
{
    public override string Description => "Parses an integer into a hexadecimal number";

    public override Argument[] ExpectedArguments { get; } =
    [
        new IntArgument("integer to parse")
    ];

    public override void Execute()
    {
        ReturnValue = new StaticTextValue(Args.GetInt("integer to parse").ToString("X"));
    }
}