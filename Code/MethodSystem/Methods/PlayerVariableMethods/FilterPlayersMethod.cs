using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerVariableMethods;

[UsedImplicitly]
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