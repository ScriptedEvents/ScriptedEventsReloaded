using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetSpectatabilityMethod : SynchronousMethod
{
    public override string Description => "Allows or disallows a player to be spectated.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new BoolArgument("new state")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var newState = Args.GetBool("new state");
        
        foreach (var player in players)
        {
            player.IsSpectatable = newState;
        }
    }
}