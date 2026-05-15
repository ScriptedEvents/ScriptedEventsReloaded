using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class CRole_CreateChanceSpawnSystemMethod : ReferenceReturningMethod<CustomRoleSpawnSystem>
{
    public override string Description => "Creates a spawn system for a custom role using a simple chance.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<RoleTypeId>("role to replace"),
        new FloatArgument("replacement chance", 0, 1, true)
        {
            Description = "The chance that a player will spawn with this custom role when changing into 'role to replace'"
        }
    ];
    
    public override void Execute()
    {
        ReturnValue = new ChanceSpawn
        {
            RoleToReplace = Args.GetEnum<RoleTypeId>("role to replace"),
            SpawnChance = Args.GetFloat("replacement chance")
        };
    }
}