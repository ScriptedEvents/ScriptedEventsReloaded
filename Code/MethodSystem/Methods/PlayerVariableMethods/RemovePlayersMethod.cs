using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerVariableMethods;

[UsedImplicitly]
public class RemovePlayersMethod : ReturningMethod<PlayerValue>
{
    public override string Description => 
        "Returns players from the original variable that were not present in other variables.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("original players"),
        new PlayersArgument("players to remove")
        {
            ConsumesRemainingValues = true,
        }
    ];
    
    public override void Execute()
    {
        var originalPlayers = Args.GetPlayers("original players");
        var playersToRemove = Args
            .GetRemainingArguments<Player[], PlayersArgument>("players to remove")
            .Flatten();

        ReturnValue = new PlayerValue(originalPlayers.Where(p => !playersToRemove.Contains(p)));
    }
}