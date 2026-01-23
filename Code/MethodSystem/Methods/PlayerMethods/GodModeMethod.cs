using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class GodmodeMethod : SynchronousMethod
{
    public override string Description => "Enables or disables godmode for specified players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new BoolArgument("mode")
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var mode = Args.GetBool("mode");

        players.ForEach(p => p.IsGodModeEnabled = mode);
    }
}