using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.TeleportMethods;

// ReSharper disable once InconsistentNaming
public class TPPositionMethod : SynchronousMethod
{
    public override string Description => "Teleports players to an XYZ position.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players to TP"),
        new FloatArgument("X coordinate"),
        new FloatArgument("Y coordinate"),
        new FloatArgument("Z coordinate")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players to TP");
        var position = new Vector3(
            Args.GetFloat("X coordinate"),
            Args.GetFloat("Y coordinate"),
            Args.GetFloat("Z coordinate"));
        
        players.ForEach(p => p.Position = position);
    }
}