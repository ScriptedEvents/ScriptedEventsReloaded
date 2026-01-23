using JetBrains.Annotations;
using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.RoleMethods;

[UsedImplicitly]
public class RoleInfoMethod : ReturningMethod<TextValue>, IReferenceResolvingMethod
{
    public Type ReferenceType => typeof(PlayerRoleBase);

    public override string Description => IReferenceResolvingMethod.Desc.Get(this);
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<PlayerRoleBase>("playerRole"),
        new OptionsArgument("property",
            Option.Enum<RoleTypeId>("type"),
            Option.Enum<Team>("team"),
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
            "name" => new TextValue(role.RoleName),
            _ => throw new AndrzejFuckedUpException("out of range")
        };
    }
}