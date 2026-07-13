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
    private readonly List<Variable> _localVariables = [];
    private bool _end = false;
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
    public string[] Arguments => ["[function name]"];

    public string Example =>
        """
        func $Add with $a $b
            return $a + $b
        end

        $sum = run $Add 5 3
        Print $sum

        func @SigmasOnly
            return Except @all @classDPlayers
        end

        @sigmas = run @SigmasOnly
        Explode @sigmas

        func ExplodeAll
            Explode @all
        end

        run ExplodeAll
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

        foreach (var (value, variableToken) in values.Zip(ExpectedVariables, (v, t) => (v, t)))
        {
            if (value.Type.IsSameOrHigherThan(variableToken.ValueType))
            {
                throw new ScriptRuntimeError(this,
                    $"Provided variable '{variableToken.Name}' of type '{value.FriendlyTypeName()}' " +
                    $"does not match expected type '{variableToken.ValueType.FriendlyTypeName()}'"
                );
            }

            _localVariables.Add(Variable.Create(variableToken.Name, value));
        }

        Script.AddLocalVariables(_localVariables.ToArray());
        return Execute();
    }

    protected override void OnReceivedControlMessageFromChild(ParentContextControlMessage msg)
    {
        switch (msg)
        {
            case Break:
                _end = true;
                return;

            case Return ret:
                _end = true;
                ReturnedValue = ret.ReturnedValue;
                return;

            default:
                SendControlMessage(msg);
                break;
        }
    }

    protected override IEnumerator<float> Execute()
    {
        _end = false;
        using var coro = RunChildren(() => _end);
        while (coro.MoveNext()) yield return coro.Current;

        _localVariables.ForEach(v => Script.RemoveLocalVariable(v));
    }
}