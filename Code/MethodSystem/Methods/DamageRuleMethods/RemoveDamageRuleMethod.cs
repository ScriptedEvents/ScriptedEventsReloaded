using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DamageRuleMethods;

[UsedImplicitly]
public class RemoveDamageRuleMethod : SynchronousMethod
{
    public override string Description => "Prevents a given damage rule from applying.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("id to remove")
        {
            DefaultValue = new(null, "Removes all damage rules")
        }
    ];
    
    public override void Execute()
    {
        DamageRuleHandler.RemoveRule(Args.GetText("id to remove"));
    }
}
