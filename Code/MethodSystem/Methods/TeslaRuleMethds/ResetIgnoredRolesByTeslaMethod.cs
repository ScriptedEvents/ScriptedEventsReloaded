using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.TeslaRuleMethds;

[UsedImplicitly]
public class ResetIgnoredRolesByTeslaMethod : SynchronousMethod
{
    public override string Description => "Resets the list of roles ignored by a tesla.";

    public override Argument[] ExpectedArguments { get; } = [];

    public override void Execute()
    {
        TeslaRuleHandler.IgnoredRoles.Clear();
    }
}
