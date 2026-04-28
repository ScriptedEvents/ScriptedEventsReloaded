using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
public class ProceduralSpawnSystemMethod : ReferenceReturningMethod<CustomRoleSpawnSystem>, IAdditionalDescription
{
    public override string Description => "Creates a procedural spawn system for a custom role.";
    public string AdditionalDescription => "This spawn system works only when the round starts.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<RoleTypeId>("role to replace")
        {
            Description = "This decides which base-game role will be replaced by a custom role."
        },
        new FloatArgument("per-player spawn chance", 0, 1, true)
        {
            Description = "Example: setting conversion chance to 30% means that EACH player has a 30% chance to convert."
        },
        new IntArgument("conversion limit")
        {
            Description = "The maximum amount of players that can convert.",
            DefaultValue = new(null, "no limit")
        },
        new IntArgument("minimum required players")
        {
            Description = "The minimum amount of players that must have the 'role to replace' " +
                          "when the round starts for the conversion to happen.",
            DefaultValue = new(null, "no minimum")
        }
    ];

    public override void Execute()
    {
        ReturnValue = new ProceduralSpawn
        {
            RoleToReplace = Args.GetEnum<RoleTypeId>("role to replace"),
            SpawnChance = Args.GetFloat("per-player spawn chance"),
            MaxAmountToSpawn = Args.GetNullableInt("conversion limit"),
            StartSpawningWhen = Args.GetNullableInt("minimum required players")
        };
    }
}