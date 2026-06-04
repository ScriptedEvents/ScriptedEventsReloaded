using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem.Other;
using SER.Code.VariableSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.MethodSystem.Methods.VariableMethods;

[UsedImplicitly]
public class GetVariableByNameMethod : ReturningMethod, ICanError
{
    public override string Description => "Returns the value of a variable with the given name and prefix.";

    public string[] ErrorReasons =>
    [
        "Provided argument is not a syntactically valid variable name.",
        "Specified variable doesn't exist"
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("variable name")
    ];
    
    public override void Execute()
    {
        var variableName = Args.GetText("variable name");

        if (Tokenizer.GetTokenFromString(variableName, Script, null)
            .HasErrored(out var error, out var token))
            throw new TosoksFuckedUpException(error);
        
        if (token is not VariableToken variableToken)
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        
        ReturnValue =
            Script.LocalVariables
                .FirstOrDefault(variable => Variable.AreSyntacticallySame(variable, variableToken))?.BaseValue
            ?? VariableIndex.GlobalVariables
                .FirstOrDefault(global => Variable.AreSyntacticallySame(global, variableToken))?.BaseValue
            ?? throw new ScriptRuntimeError(this, ErrorReasons[1]);
    }

    public override TypeOfValue Returns { get; } = new UnknownTypeOfValue();
}