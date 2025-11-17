using JetBrains.Annotations;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.PlayerVariableMethods;

[UsedImplicitly]
public class ContainsPlayerMethod : ReturningMethod<BoolValue>
{
    public override string Description =>
        "Returns a true/false value indicating if the provided player is in the list.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("player list")
        {
            Description = "The variable that will be checked if it contains the player."
        },
        new PlayerArgument("searched player")
    ];
    
    public override void Execute()
    {
        var playerList = Args.GetPlayers("player list");
        var searchedPlayer = Args.GetPlayer("searched player");
        ReturnValue = new(playerList.Contains(searchedPlayer));
    }
}