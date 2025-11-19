using LabApi.Features.Wrappers;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Extensions;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.PlayerVariableMethods;

public class JoinPlayersMethod : ReturningMethod<PlayerValue>
{
    public override string Description =>
        "Returns all players that were provided from multiple player variables.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players")
        {
            ConsumesRemainingValues = true
        }
    ];
    
    public override void Execute()
    {
        ReturnValue = new PlayerValue(Args
            .GetRemainingArguments<Player[], PlayersArgument>("players")
            .Flatten()
            .Distinct()
            .ToArray()
        );
    }
}