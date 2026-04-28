using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
public class IsCRoleRegisteredMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Checks if a given custom role is registered.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("role id", false)
    ];
    
    public override void Execute()
    {
        ReturnValue = CRole.RegisteredRoles.ContainsKey(Args.GetText("role id"));
    }
}