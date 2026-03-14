using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class ShowHitMarkerMethod : SynchronousMethod
{
    public override string Description =>
        "Shows a hit marker to players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new FloatArgument("hitmarker size") 
            { DefaultValue = new(1f, "default size") }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var size = Args.GetFloat("hitmarker size");
        
        players.ForEach(plr => plr.SendHitMarker(size));
    }
}