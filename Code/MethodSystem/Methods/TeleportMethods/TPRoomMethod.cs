using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.TeleportMethods;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class TPRoomMethod : SynchronousMethod
{
    public override string Description => "Teleports players to relative coordinates of a room.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players to teleport"),
        new RoomArgument("room to teleport to"),
        new FloatArgument("relative x")
        {
            DefaultValue = new(0, null)
        },
        new FloatArgument("relative y")
        {
            DefaultValue = new(0, null)
        },
        new FloatArgument("relative z")
        {
            DefaultValue = new(0, null)
        },
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players to teleport");
        var room = Args.GetRoom("room to teleport to");
        var pos = room.Transform.TransformPoint(new(
            Args.GetFloat("relative x"),
            Args.GetFloat("relative y"),
            Args.GetFloat("relative z")));

        players.ForEach(plr => plr.Position = pos + new Vector3(0, plr.Scale.y + 0.01f, 0));
    }
}