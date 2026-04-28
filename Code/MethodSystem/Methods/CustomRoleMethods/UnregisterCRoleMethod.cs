using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
public class UnregisterCRoleMethod : SynchronousMethod
{
    public override string Description => "Unregisters a given custom role from the server.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CustomRoleArgument("custom role")
    ];
    
    public override void Execute()
    {
        CRole.RegisteredRoles.Remove(Args.GetCustomRole("custom role").Id);
    }
}