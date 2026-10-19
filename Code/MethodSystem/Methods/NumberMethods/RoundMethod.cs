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
        new ValueArgument<NumberValue>("number to round")
        {
            Description = "Any number."
        },
        new IntArgument("decimal places", 0, 28)
        {
            DefaultValue = new(0, "rounds to whole number (0)"),
            Description = "The number of decimal places to round to, from 0 to 28. Halves round away from zero, so 2.5 becomes 3."
        }
    ];
    
    public override void Execute()
    {
        // Round the exact number, so whole numbers stay unchanged and 1.005 becomes 1.01.
        ReturnValue = Math.Round(
            Args.GetValue<NumberValue>("number to round").Value,
            Args.GetInt("decimal places"), 
            MidpointRounding.AwayFromZero
        );
    }
}