using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ScriptSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.MethodSystem.Methods.ScriptMethods;

[UsedImplicitly]
public class RunScriptMethod : SynchronousMethod
{
    public override string Description => "Runs a script.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CreatedScriptArgument("script to create"),
        new VariableArgument("variables to pass")
        {
            ConsumesRemainingValues = true,
            DefaultValue = new(new List<Variable>(), "none"),
            Description = "Passes an exact copy of the provided variables to the script."
        }
    ];
    
    public override void Execute()
    {
        var script = Args.GetCreatedScript("script to create");
        var variables = Args.GetRemainingArguments<Variable, VariableArgument>("variables to pass");
        
        script.AddVariables(variables);
        script.Run(RunContext.Script, Script);
    }
}