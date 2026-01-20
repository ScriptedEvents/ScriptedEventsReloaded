using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.MethodSystem.Methods.VariableMethods;

[UsedImplicitly]
public class LogVarMethod : ReturningMethod<TextValue>
{
    public override string Description => "Returns a formatted version of the variable for logging purposes.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new VariableArgument("variable")
    ];
    
    public override void Execute()
    {
        var variable = Args.GetVariable("variable");
        ReturnValue = $"{variable} = {(variable is LiteralVariable lv ? lv.Value : variable.BaseValue)}";
    }
}