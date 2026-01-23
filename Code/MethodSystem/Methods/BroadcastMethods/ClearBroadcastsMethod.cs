using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.BroadcastMethods;

[UsedImplicitly]
public class ClearBroadcastsMethod : SynchronousMethod
{
    public override string Description => "Clears broadcasts for players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");

        foreach (var plr in players)
        {
            plr.ClearBroadcasts();
        }
    }
}