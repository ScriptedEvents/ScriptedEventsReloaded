using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.MethodSystem.Methods.ScriptMethods;

[UsedImplicitly]
public class TransferVariablesMethod : SynchronousMethod
{
    public override string Description => "Makes a copy of the given local variable(s) in a different script.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new RunningScriptArgument("target script"),
        new VariableArgument("variables")
        {
            ConsumesRemainingValues = true,
        }
    ];

    public override void Execute()
    {
        var script = Args.GetRunningScript("target script");
        var variables = Args.GetRemainingArguments<Variable, VariableArgument>("variables");
        
        script.AddVariables(variables);
    }
}