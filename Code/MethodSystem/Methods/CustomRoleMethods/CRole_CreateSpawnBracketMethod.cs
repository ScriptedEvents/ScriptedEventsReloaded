using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class CRole_CreateSpawnBracketMethod : ReferenceReturningMethod<SpawnBracket>, IAdditionalDescription, ICanError
{
    public override string Description => "Creates a spawn bracket for a the bracket spawn system.";

    public string AdditionalDescription =>
        "Example: If you define a bracket with lower bound 3 and upper bound 5, and amount to spawn 2, then if there " +
        "are 3 to 5 players that can be converted to a custom role, then 2 of them will be converted. " +
        "If the players available to convert exceed the range of 3 to 5, then this bracket is ignored.";

    public string[] ErrorReasons { get; } =
    [
        "The lower bound is greater than the upper bound.",
        "The amount to spawn is greater than the smallest amount of players available to convert."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new IntArgument("lower bound", 0)
        {
            Description = "The amount of players from which this bracket applies."
        },
        new IntArgument("upper bound", 0)
        {
            Description = "The amount of players until which this bracket applies."
        },
        new IntArgument("amount to spawn", 0)
        {
            Description = "The amount of players that will be spawned in this bracket."
        }
    ];

    public override void Execute()
    {
        var lower = Args.GetInt("lower bound");
        var upper = Args.GetInt("upper bound");
        var amount = Args.GetInt("amount to spawn");

        if (lower > upper)
        {
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        }

        if (amount > lower)
        {
            throw new ScriptRuntimeError(this, ErrorReasons[1]);
        }

        ReturnValue = new()
        {
            LowerBound = lower,
            UpperBound = upper,
            AmountToSpawn = amount
        };
    }
}