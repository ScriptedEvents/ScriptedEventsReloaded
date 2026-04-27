using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerVariableMethods;

[UsedImplicitly]
public class TakeMethod : ReturningMethod<PlayerValue>
{
    public override string Description =>
        "Takes a specified amount of players from a player variable, lower or equal to the limit.";

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