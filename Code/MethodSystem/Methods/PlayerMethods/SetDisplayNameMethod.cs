using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetDisplayNameMethod : SynchronousMethod
{
    public override string Description => "Sets display name for specified players";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new TextArgument("display name")
        {
            Description = "Leave empty quotes if you want to remove display name from players"
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var displayName = Args.GetText("display name");
        
        players.ForEach(p => p.DisplayName = displayName);
    }
}
