using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.NumberMethods;

[UsedImplicitly]
public class RoundMethod : ReturningMethod<NumberValue>
{
    public override string Description => "Rounds a provided number.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new FloatArgument("number to round"),
        new IntArgument("decimal places", 0)
        {
            DefaultValue = new(0, "rounds to whole number (0)"),
            Description = "The number of decimal places to round to."
        }
    ];
    
    public override void Execute()
    {
        ReturnValue = (decimal)Math.Round(
            Args.GetFloat("number to round"),
            Args.GetInt("decimal places"), 
            MidpointRounding.AwayFromZero
        );
    }
}