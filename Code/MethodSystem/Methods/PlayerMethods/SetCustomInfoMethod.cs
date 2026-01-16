using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetCustomInfoMethod : SynchronousMethod
{
    public override string Description =>
        "Sets custom info (overhead text) for specific players, visible to specific target players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("playersToAffect"),
        new TextArgument("customInfoText"),
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("playersToAffect");
        var text = Args.GetText("customInfoText")
            .Replace("\\n", "\n")
            .Replace("<br>", "\n");

        foreach (var player in players)
        {
            player.CustomInfo = text;
        }
    }
}