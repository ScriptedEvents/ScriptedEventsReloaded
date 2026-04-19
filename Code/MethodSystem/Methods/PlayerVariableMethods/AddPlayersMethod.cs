using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.MethodSystem.Methods.PlayerVariableMethods;

[UsedImplicitly]
public class AddPlayersMethod : SynchronousMethod
{
    public override string Description => "Adds players to a player variable.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new VariableArgument<PlayerVariable>("player variable"),
        new PlayersArgument("players to add")
    ];
    
    public override void Execute()
    {
        var playerVariable = Args.GetVariable<PlayerVariable>("player variable");
        var playersToAdd = Args.GetPlayers("players to add");
        
        playerVariable.Value.Players = playerVariable.Players.Concat(playersToAdd).ToArray();
    }
}