using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerValueMethods;

[UsedImplicitly]
public class JoinMethod : ReturningMethod<PlayerValue>
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