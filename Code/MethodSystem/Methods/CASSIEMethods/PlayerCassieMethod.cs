using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.CASSIEMethods;

[UsedImplicitly]
public class PlayerCassieMethod : SynchronousMethod
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
        var labApiPlayers = Args.GetPlayers("players");
        var isNoisy = Args.GetOption("mode") == "jingle";
        var message = Args.GetText("message");
        var subtitles = Args.GetText("subtitles");

        foreach (var player in labApiPlayers)
        {
            player.SendCassieMessage(
                message, 
                subtitles,
                isNoisy, 
                1
            );
        }
    }
}