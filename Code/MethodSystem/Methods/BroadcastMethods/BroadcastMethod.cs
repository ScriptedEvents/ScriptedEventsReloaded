using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.BroadcastMethods;

public class BroadcastMethod : SynchronousMethod, IEssential, ICanError
{
    public override string Description => "Sends a broadcast to players.";

    public string[] ErrorReasons { get; } =
    [
        $"A broadcast can last at most {ushort.MaxValue} seconds (about 18 hours). Use a shorter duration."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new DurationArgument("duration")
        {
            Description = $"Broadcasts are shown in whole seconds, so a part of a second rounds up (500ms shows for 1 second). " +
                          $"The longest broadcast is {ushort.MaxValue} seconds (about 18 hours)."
        },
        new TextArgument("message")
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var duration = Args.GetDuration("duration");
        var message = Args.GetText("message");

        var seconds = Math.Ceiling(duration.TotalSeconds);
        if (seconds > ushort.MaxValue)
            throw new ScriptRuntimeError(this, ErrorReasons[0]);

        foreach (var player in players) 
            Server.SendBroadcast(player, message, (ushort)Math.Max(0, seconds));
    }
}