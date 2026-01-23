using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class KickMethod : SynchronousMethod
{
    public override string Description => "Kicks players from the server.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new TextArgument("reason")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var reason = Args.GetText("reason");
        
        players.ForEach(p => p.Kick(reason));
    }
}