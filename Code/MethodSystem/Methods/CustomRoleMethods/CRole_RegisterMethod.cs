using LabApi.Features.Wrappers;
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
public class CRole_RegisterMethod : SynchronousMethod, ICanError
{
    public override string Description => "Registers a custom role.";

    public string[] ErrorReasons =>
    [
        "Display name uses an invalid format."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("id", false)
        {
            Description = "This will be used to identify the role."
        },
        new TextArgument("display name"),
        new EnumArgument<RoleTypeId>("role type"),
        new ReferenceArgument<CustomRoleSpawnSystem>("spawn system")
        {
            DefaultValue = new(null, "the role will not be spawned automatically")
        },
        new BoolArgument("remove role on death")
        {
            DefaultValue = new(true, null),
        }
    ];

    public override void Execute()
    {
        var id = Args.GetText("id");
        
        var displayName = Args.GetText("display name");
        if (!Player.ValidateCustomInfo(displayName, out var reason))
        {
            throw new ScriptRuntimeError(this, $"Display name uses an invalid format: {reason}");
        }

        CRole.Register(new CRole
        {
            Id = id,
            DisplayName = displayName,
            RoleType = Args.GetEnum<RoleTypeId>("role type"),
            RemoveRoleOnDeath = Args.GetBool("remove role on death"),
            SpawnSystem = Args.GetReference<CustomRoleSpawnSystem>("spawn system")
        });
    }
}
