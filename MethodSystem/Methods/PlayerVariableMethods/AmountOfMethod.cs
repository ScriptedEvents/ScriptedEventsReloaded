using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.PlayerVariableMethods;

public class AmountOfMethod : ReturningMethod<NumberValue>
{
    public override string Description => "Returns the amount of players in a given player variable.";
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("variable")
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetPlayers("variable").Length;
    }
}