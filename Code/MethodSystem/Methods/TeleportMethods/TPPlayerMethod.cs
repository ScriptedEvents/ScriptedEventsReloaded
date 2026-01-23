using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.TeleportMethods;

// ReSharper disable once InconsistentNaming
public class TPPlayerMethod : SynchronousMethod
{
    public override string Description => "Teleports players to another player.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players to TP"),
        new PlayerArgument("player target")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players to TP");
        var target = Args.GetPlayer("player target");
        
        players.ForEach(p => p.Position = target.Position);
    }
}