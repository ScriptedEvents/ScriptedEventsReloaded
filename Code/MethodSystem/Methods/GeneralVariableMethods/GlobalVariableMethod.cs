using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem;

namespace SER.Code.MethodSystem.Methods.GeneralVariableMethods;

[UsedImplicitly]
public class GlobalVariableMethod : SynchronousMethod, ICanError
{
    public override string Description => "Makes a provided local variable into a global variable.";

    public string[] ErrorReasons =>
    [
        "Provided variable does not exist."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new VariableArgument("variable to make global")
    ];

    public override void Execute()
    {
        var variable = Args.GetVariable("variable to make global");
        VariableIndex.GlobalVariables.RemoveAll(existingVar => existingVar.Name == variable.Name);
        VariableIndex.GlobalVariables.Add(variable);
    }
}