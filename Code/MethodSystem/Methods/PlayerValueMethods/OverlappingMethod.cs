using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerValueMethods;

[UsedImplicitly]
public class OverlappingMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Checks if provided player variables have the exact same players, in any order.";

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
        
        // Compare who is in each list; order and repeated players do not matter.
        // Player does not override Equals, so this compares the actual player objects.
        var firstPlayers = new HashSet<Player>(variables[0]);
        ReturnValue = variables.Skip(1).All(v => firstPlayers.SetEquals(v));
    }
}