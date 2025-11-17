using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.PlayerMethods;

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
