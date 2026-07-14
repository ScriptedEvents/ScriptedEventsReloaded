using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class CRole_CreateBracketSpawnSystemMethod : ReferenceReturningMethod<CustomRoleSpawnSystem>, ICanError
{
    public override string Description => "Creates a spawn for a custom roles that uses ranges of available players.";

    public string[] ErrorReasons { get; } =
    [
        "At least one spawn bracket is required.",
        "Brackets are overlapping with each other."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<RoleTypeId>("role to replace"),
        new ReferenceArgument<SpawnBracket>("spawn brackets")
        {
            Description = "The spawn brackets to use for this role.",
            ConsumesRemainingValues = true
        }
    ];

    public override void Execute()
    {
        var roleToReplace = Args.GetEnum<RoleTypeId>("role to replace");
        var spawnBrackets = Args
            .GetRemainingArguments<SpawnBracket, ReferenceArgument<SpawnBracket>>("spawn brackets");

        if (spawnBrackets.Length == 0)
        {
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        }

        for (var firstIndex = 0; firstIndex < spawnBrackets.Length; firstIndex++)
        {
            for (var secondIndex = firstIndex + 1; secondIndex < spawnBrackets.Length; secondIndex++)
            {
                var first = spawnBrackets[firstIndex];
                var second = spawnBrackets[secondIndex];
                if (first.LowerBound <= second.UpperBound && second.LowerBound <= first.UpperBound)
                {
                    throw new ScriptRuntimeError(
                        this,
                        $"Spawn brackets {first.LowerBound}-{first.UpperBound} and " +
                        $"{second.LowerBound}-{second.UpperBound} overlap."
                    );
                }
            }
        }

        ReturnValue = new BracketSpawn
        {
            RoleToReplace = roleToReplace,
            SpawnBrackets = spawnBrackets
        };
    }
}
