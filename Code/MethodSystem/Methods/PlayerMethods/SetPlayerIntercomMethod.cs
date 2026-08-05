using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using Intercom = PlayerRoles.Voice.Intercom;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetPlayerIntercomMethod : SynchronousMethod
{
    public override string Description => "Sets whether a player is using a global intercom.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new BoolArgument("state")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var state = Args.GetBool("state");
        
        foreach (var player in players)
        {
            Intercom.TrySetOverride(player.ReferenceHub, state);
        }
    }
}