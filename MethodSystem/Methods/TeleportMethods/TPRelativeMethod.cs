using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Extensions;
using SER.MethodSystem.BaseMethods;
using UnityEngine;

namespace SER.MethodSystem.Methods.TeleportMethods;

// ReSharper disable once InconsistentNaming
public class TPRelativeMethod : SynchronousMethod
{
    public override string Description => "Teleports players to relative coordinates of a room.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new RoomArgument("room"),
        new FloatArgument("relative x"),
        new FloatArgument("relative y"),
        new FloatArgument("relative z")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var room = Args.GetRoom("room");
        var pos = room.Transform.TransformPoint(new(
            Args.GetFloat("relative x"),
            Args.GetFloat("relative y"),
            Args.GetFloat("relative z")));

        players.ForEach(plr => plr.Position = pos + new Vector3(0, plr.Scale.y + 0.01f, 0));
    }
}