using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;
using SER.TokenSystem.Tokens.VariableTokens;
using SER.VariableSystem;

namespace SER.MethodSystem.Methods.GeneralVariableMethods;

public class GlobalVariableMethod : SynchronousMethod, ICanError
{
    public override string Description => "Makes a provided local variable into a global variable.";

    public string[] ErrorReasons =>
    [
        "Provided variable does not exist."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TokenArgument<VariableToken>("variable to make global")
    ];

    public override void Execute()
    {
        var variableToken = Args.GetToken<VariableToken>("variable to make global");
        if (variableToken.TryGetVariable().HasErrored(out var error, out var variable))
        {
            throw new ScriptRuntimeError(error);
        }

        VariableIndex.GlobalVariables.RemoveAll(existingVar => existingVar.Name == variable.Name);
        VariableIndex.GlobalVariables.Add(variable);
    }
}