using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class ResetMuteMethod : SynchronousMethod
{
    public override string Description => "Resets mute status for specified players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new BoolArgument("revoke permanent")
        {
            DefaultValue = new(true, null)
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var revokePermanent = Args.GetBool("revoke permanent");
        
        foreach (var plr in players) plr.Unmute(revokePermanent);
    }
}
