using System.Collections;
using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
public class BracketSpawnSystemMethod : ReferenceReturningMethod<CustomRoleSpawnSystem>, ICanError
{
    public override string Description => "Creates a spawn for a custom roles that uses ranges of available players.";

    public string[] ErrorReasons { get; } =
    [
        "Brackets are overlapping with each other."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<RoleTypeId>("role to replace"),
        new ReferenceArgument<SpawnBracket>("spawn brackets")
        {
            Description = "The spawn brackets to use for this role."
        }
    ];

    public override void Execute()
    {
        var roleToReplace = Args.GetEnum<RoleTypeId>("role to replace");
        var spawnBrackets = Args
            .GetRemainingArguments<SpawnBracket, ReferenceArgument<SpawnBracket>>("spawn brackets");

        var length = spawnBrackets.Max(bracket => bracket.UpperBound);

        List<BitArray> brackets = [];
        foreach (var bracket in spawnBrackets)
        {
            var checkingRange = new BitArray(length);
            for (int i = bracket.LowerBound; i <= bracket.UpperBound; i++)
            {
                checkingRange[i] = true;
            }

            brackets.Add(checkingRange);
        }

        for (int i = 0; i < length; i++)
        {
            var found = false;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var bracket in brackets)
            {
                if (!bracket[i]) continue;
                
                if (!found)
                {
                    found = true;
                    continue;
                }

                throw new ScriptRuntimeError(
                    this, 
                    $"The amount of players [{i + 1}] is being claimed by multiple brackets!"
                );
            }
        }

        ReturnValue = new BracketSpawn
        {
            RoleToReplace = roleToReplace,
            SpawnBrackets = spawnBrackets
        };
    }
}