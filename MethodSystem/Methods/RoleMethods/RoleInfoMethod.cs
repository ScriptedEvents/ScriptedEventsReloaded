using System;
using PlayerRoles;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.ArgumentSystem.Structures;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.RoleMethods;

public class RoleInfoMethod : ReturningMethod<TextValue>, IReferenceResolvingMethod
{
    public Type ReferenceType => typeof(PlayerRoleBase);

    public override string Description => null!;
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<PlayerRoleBase>("playerRole"),
        new OptionsArgument("property",
            Option.Enum<RoleTypeId>("type"),
            Option.Enum<Team>("team"),
            Option.Enum<RoleSpawnFlags>("spawnFlags"), 
            Option.Enum<RoleChangeReason>("spawnReason"),
            "name"
        )
    ];

    public override void Execute()
    {
        var role = Args.GetReference<PlayerRoleBase>("playerRole");
        ReturnValue = Args.GetOption("property") switch
        {
            "type" => new TextValue(role.RoleTypeId.ToString()),
            "team" => new TextValue(role.Team.ToString()),
            "spawnflags" => new TextValue(role.ServerSpawnFlags.ToString()),
            "spawnreason" => new TextValue(role.ServerSpawnReason.ToString()),
            "name" => new TextValue(role.RoleName),
            _ => throw new AndrzejFuckedUpException("out of range")
        };
    }
}