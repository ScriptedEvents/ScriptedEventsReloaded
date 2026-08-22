using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerValueMethods;

[UsedImplicitly]
public class IntersectMethod : ReturningMethod<PlayerValue>
{
    public override string Description =>
        "Returns players that are present in every provided player variable.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("first value"),
        new PlayersArgument("other values")
        {
            ConsumesRemainingValues = true
        }
    ];

    public override void Execute()
    {
        var firstValue = Args.GetPlayers("first value");
        var otherValues = Args
            .GetRemainingArguments<Player[], PlayersArgument>("other values")
            .Select(players => players.ToHashSet())
            .ToArray();

        ReturnValue = new PlayerValue(
            firstValue.Where(player => otherValues.All(players => players.Contains(player))));
    }
}
