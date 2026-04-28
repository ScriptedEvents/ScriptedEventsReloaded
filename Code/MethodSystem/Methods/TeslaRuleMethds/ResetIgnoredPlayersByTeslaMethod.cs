using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.TeslaRuleMethds;

[UsedImplicitly]
public class ResetIgnoredPlayersByTeslaMethod : SynchronousMethod
{
    public override string Description => "Resets the list of players ignored by a tesla.";

    public override Argument[] ExpectedArguments { get; } = [];

    public override void Execute()
    {
        TeslaRuleHandler.IgnoredPlayerIds.Clear();
    }
}
