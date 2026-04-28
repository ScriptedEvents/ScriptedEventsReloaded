using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class MuteMethod : SynchronousMethod
{
    public override string Description => "Mutes specified players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new BoolArgument("is temporary")
        {
            DefaultValue = new(true, null)
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var isTemporary = Args.GetBool("is temporary");
        
        foreach (var plr in players) plr.Mute(isTemporary);
    }
}