using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerVariableMethods;

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