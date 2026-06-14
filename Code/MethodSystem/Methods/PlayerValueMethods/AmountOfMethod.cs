using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerValueMethods;

[UsedImplicitly]
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