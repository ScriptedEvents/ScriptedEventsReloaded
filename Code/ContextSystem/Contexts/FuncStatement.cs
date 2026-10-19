using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class FuncStatement :
    StatementContext,
    INotRunningContext,
    IAcceptOptionalVariableDefinitionsContext,
    IMayReturnValueContext,
    IKeywordContext
{
    public VariableToken[] ExpectedVariables = [];
    public string FunctionName { get; private set; } = null!;

    public override string FriendlyName =>
        FunctionName is not null
            ? $"'{FunctionName}' function definition statement"
            : "function definition statement";

    public Result SetOptionalVariables(params VariableToken[] variableTokens)
    {
        ExpectedVariables = variableTokens;
        return true;
    }

    public string KeywordName => "func";
    public string Description => "Defines a function.";
    public ContextArgument[] Arguments => ["[function name]"];

    public string Example =>
        """
        func $Add with $left $right
            return $left + $right
        end

        $sum = run $Add 5 3
        Print "5 + 3 = {$sum}"

        # Functions without parameters can be called by name.
        func Announce
            Print "The round has started."
        end
        run Announce
        """;

    // gets the type of value associated with a token type of a variable prefix
    // sketchy!!
    public TypeOfValue? Returns
    {
        get
        {
            var varTypeToken = VariableToken.VariablePrefixes
                .FirstOrDefault(pair => pair.prefix == FunctionName.FirstOrDefault())
                .varTypeToken;

            return varTypeToken?.CreateInstance<VariableToken>().ValueType;
        }
    }

    /// <summary>
    ///     The value returned by the most recently finished call, or null if that call did not use 'return'.
    /// </summary>
    public Value? ReturnedValue { get; private set; }

    public string MissingValueHint => "Maybe you forgot to use the 'return' keyword?";
    public string UndefinedReturnsHint => "Maybe you forgot to define the return type in the function name?";

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token.GetType() != typeof(BaseToken) && token is not VariableToken)
        {
            return TryAddTokenRes.Error(
                $"Value '{token.RawRep}' cannot represent a function name, " +
                $"as it was recognized as a {token.FriendlyTypeName()}"
            );
        }

        FunctionName = token.RawRep;
        Script.DefineFunction(FunctionName, this);
        return TryAddTokenRes.End();
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            !string.IsNullOrWhiteSpace(FunctionName),
            "Function name was not provided."
        );
    }

    public IEnumerator<float> RunProperly(params Value[] values)
    {
        if (LineNum.HasValue)
            Script.CurrentLine = LineNum.Value;

        if (values.Length != ExpectedVariables.Length)
        {
            throw new ScriptRuntimeError(this,
                $"Provided [{values.Length}] values, but [{ExpectedVariables.Length}] were expected."
            );
        }

        // Each call gets its own parameter variables, so repeated calls do not pile up old arguments.
        var parameters = new Variable[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            var value = values[i];
            var variableToken = ExpectedVariables[i];
            if (!value.Type.IsSameOrHigherThan(variableToken.ValueType))
            {
                throw new ScriptRuntimeError(this,
                    $"Provided variable '{variableToken.Name}' of type '{value.Type}' " +
                    $"does not match expected type '{variableToken.ValueType}'"
                );
            }

            parameters[i] = Variable.Create(variableToken.Name, value);
        }

        // Forget the previous call's answer, so a call that ends without 'return' reports the missing value
        // instead of reusing an old one.
        ReturnedValue = null;
        return RunCall(parameters);
    }

    private IEnumerator<float> RunCall(Variable[] parameters)
    {
        // A parameter can hide a script variable with the same name (for example when the function calls
        // itself). Remember those variables so they can be put back when this call ends.
        var hidden = new Variable?[parameters.Length];
        var added = 0;
        var body = new BodyRun();
        try
        {
            for (; added < parameters.Length; added++)
            {
                var parameter = parameters[added];
                hidden[added] = Script.LocalVariables.FirstOrDefault(v =>
                    v.Prefix == parameter.Prefix && v.Name == parameter.Name);
                Script.AddLocalVariable(parameter);
            }

            using var coro = RunChildren(body);
            while (coro.MoveNext()) yield return coro.Current;
        }
        finally
        {
            // Undo in reverse order, so two parameters with the same name are restored correctly.
            for (var i = added - 1; i >= 0; i--)
            {
                if (hidden[i] is { } previous)
                {
                    Script.AddLocalVariable(previous);
                }
                else
                {
                    Script.RemoveLocalVariable(parameters[i]);
                }
            }
        }

        ReturnedValue = body.StoppedBy is Return ret ? ret.ReturnedValue : null;
    }

    protected override void OnReceivedControlMessageFromChild(ParentContextControlMessage msg)
    {
        switch (msg)
        {
            // 'return' and 'break' end the function. The body has already stopped running,
            // and RunCall reads the returned value from it.
            case Break:
            case Return:
                return;

            // Scripts that compile cannot reach this, because 'continue' outside a loop is rejected when
            // the script loads. Fail with a script error instead of looping forever if it ever happens.
            default:
                throw new ScriptRuntimeError(this,
                    "'continue' was used outside a loop in this function. " +
                    "Use 'continue' only inside 'repeat', 'over', 'while' or 'forever'. " +
                    "To leave the function early, use 'return' instead."
                );
        }
    }

    // Functions never run in place (see INotRunningContext); they only run through 'run' calls.
    protected override IEnumerator<float> Execute()
    {
        return RunProperly();
    }
}
