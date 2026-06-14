using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerValueMethods;

[UsedImplicitly]
public class OverlappingMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Checks if provided player variables have the exact same players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        // this is only for having 2 arguments be required
        new PlayersArgument("first value"),
        new PlayersArgument("other values")
        {
            ConsumesRemainingValues = true
        }
    ];
    
    public override void Execute()
    {
        var variables = Args
            .GetRemainingArguments<Player[], PlayersArgument>("other values")
            .Concat([Args.GetPlayers("first value")])
            .ToArray();
        
        int startHash = variables[0].GetEnumerableHashCode();
        ReturnValue = variables.Skip(1).All(v => v.GetEnumerableHashCode() == startHash);
    }
}