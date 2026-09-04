using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Integrations.Ucr;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.UCRMethods;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class UCR_GetPlayersWithRoleMethod : ReturningMethod<PlayerValue>, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.UncomplicatedCustomRoles;

    public override string Description => "Gets all players who have a provided UCR role.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new IntArgument("role id")
    ];

    public override void Execute()
    {
        ReturnValue = UcrBridge.GetPlayersWithRole(Args.GetInt("role id")).ToPlayerValue();
    }
}
