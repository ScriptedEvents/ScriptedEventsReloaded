using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.Integrations.Ucr;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.UCRMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class UCR_GetRoleMethod : ReferenceReturningMethod<UCRRole>, IAdditionalDescription, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.UncomplicatedCustomRoles;
    
    public override string Description => "Returns a reference to the UCR role a player has.";

    public string AdditionalDescription =>
        "Be sure to verify if the player has a role. The reference will be INVALID when the player doesn't have a role.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player")
    ];

    public override void Execute()
    {
        ReturnValue = UcrBridge.GetRole(Args.GetPlayer("player"))!;
    }
}
