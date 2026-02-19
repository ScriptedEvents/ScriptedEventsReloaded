using JetBrains.Annotations;
using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
using UncomplicatedCustomRoles.API.Interfaces;

namespace SER.Code.MethodSystem.Methods.UCRMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class UCRRoleInfoMethod : LiteralValueReturningMethod, IReferenceResolvingMethod, IDependOnFramework
{
    public Type ResolvesReference => typeof(ICustomRole);

    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.Ucr;

    public override string Description => "Returns information about a custom role.";

    public override TypeOfValue LiteralReturnTypes => new TypesOfValue(
        typeof(NumberValue), 
        typeof(TextValue)
    );

    public override Argument[] ExpectedArguments =>
    [
        new LooseReferenceArgument("custom role reference", typeof(ICustomRole)),
        new OptionsArgument("property", 
            "id",
            "name",
            Option.Enum<RoleTypeId>("role"),
            Option.Enum<Team>("team"), 
            Option.Enum<RoleTypeId>("roleAppearance")
        )
    ];

    public override void Execute()
    {
        var role = Args.GetLooseReference<ICustomRole>("custom role reference");
        
        ReturnValue = Args.GetOption("property") switch
        {
            "id" => new NumberValue(role.Id),
            "name" => new StaticTextValue(role.Name),
            "role" => role.Role.ToString().ToStaticTextValue(),
            "team" => role.Team.ToString().ToStaticTextValue(),
            "roleAppearance" => role.RoleAppearance.ToString().ToStaticTextValue(),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}