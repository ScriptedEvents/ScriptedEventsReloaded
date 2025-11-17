using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.TokenSystem.Tokens.ExpressionTokens;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.PlayerVariableMethods;

public class FilterPlayersMethod : ReturningMethod<PlayerValue>
{
    public override string Description => "Returns players which match the value for a given property.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players to filter"),
        new EnumArgument<PlayerExpressionToken.PlayerProperty>("player property"),
        new AnyValueArgument("desired value")
    ];
    
    public override void Execute()
    {
        var playersToFilter = Args.GetPlayers("players to filter");
        var playerProperty = Args.GetEnum<PlayerExpressionToken.PlayerProperty>("player property");
        var desiredValue = Args.GetAnyValue("desired value");
        var handler = PlayerExpressionToken.PropertyInfoMap[playerProperty].Handler;

        ReturnValue = new(playersToFilter.Where(p => handler(p) == desiredValue));
    }
}