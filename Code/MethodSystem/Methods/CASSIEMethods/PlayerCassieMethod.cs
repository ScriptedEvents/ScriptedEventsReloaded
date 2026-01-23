using Exiled.API.Extensions;
using Exiled.API.Features;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.CASSIEMethods;

public class PlayerCassieMethod : SynchronousMethod, IExiledMethod
{
    public override string Description => "Makes a CASSIE announcement to specified players only.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new OptionsArgument("mode",
            "jingle",
            "noJingle"
        ),
        new TextArgument("message"),
        new TextArgument("subtitles")
        {
            DefaultValue = new("", "empty"),
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var isNoisy = Args.GetOption("mode") == "jingle";
        var message = Args.GetText("message");
        var subtitles = Args.GetText("subtitles");

        foreach (var player in players.Select(Player.Get))
        {
            player.MessageTranslated(
                message, 
                subtitles, 
                subtitles,
                isNoisy, 
                !string.IsNullOrWhiteSpace(subtitles)
            );
        }
    }
}