using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerVariableMethods;

[UsedImplicitly]
public class LimitPlayersMethod : ReturningMethod<PlayerValue>
{
    public override string Description =>
        "Returns a player variable with amount of players being equal or lower than the limit.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new IntArgument("limit", 1)   
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players").ToList();
        var limit = Args.GetInt("limit");

        while (players.Len > limit && players.Len > 0)
        {
            players.PullRandomItem();
        }
        
        ReturnValue = new PlayerValue(players);
    }
}