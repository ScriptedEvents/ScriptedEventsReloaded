using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem;

namespace SER.Code.MethodSystem.Methods.GeneralVariableMethods;

[UsedImplicitly]
public class PopVariableMethod : ReturningMethod
{
    public override string Description => "Erases a given variable, returning its value.";

    public override TypeOfValue Returns => new UnknownTypeOfValue();

    public override Argument[] ExpectedArguments { get; } =
    [  
        new OptionsArgument("target", "local", "global"),
        new VariableArgument("variable to remove")
        {
            Description = "This only works on local variables!"
        }
    ];

    public override void Execute()
    {
        var variable = Args.GetVariable("variable to remove");
        switch (Args.GetOption("target"))
        {
            case "local": 
                Script.RemoveVariable(variable);
                break;
            case "global":
                VariableIndex.GlobalVariables.RemoveAll(existingVar => existingVar.Name == variable.Name);
                break;
        }
        
        ReturnValue = variable.BaseValue;
    }
}