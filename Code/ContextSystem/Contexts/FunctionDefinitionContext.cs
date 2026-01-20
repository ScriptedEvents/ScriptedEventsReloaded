using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.CommunicationInterfaces;
using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class FunctionDefinitionContext :
    StatementContext,
    INotRunningContext, 
    IAcceptOptionalVariableDefinitions,
    IMayReturnValueContext,
    IKeywordContext
{
    public string FunctionName { get; private set;  } = null!;
    private bool _end = false;
    private VariableToken[] _expectedVariables = [];
    private readonly List<Variable> _localVariables = [];

    public string KeywordName => "func";
    public string Description => "Defines a function.";
    public string[] Arguments => ["[function name]"];
    
    // gets the type of value associated with a token type of a variable prefix
    // sketchy!!
    public TypeOfValue? Returns
    {
        get
        {
            var varTypeToken = VariableToken.VariablePrefixes
                .FirstOrDefault(pair => pair.prefix == FunctionName.FirstOrDefault())
                .varTypeToken;

            if (varTypeToken == null) return null;

            return new SingleTypeOfValue(varTypeToken.CreateInstance<VariableToken>().ValueType);
        }
    }

    public Value? ReturnedValue { get; private set; }

    public string MissingValueHint => "Maybe you forgot to use the 'return' keyword?";
    public string UndefinedReturnsHint => "Maybe you forgot to define the return type in the function name?";

    protected override string FriendlyName =>
        FunctionName is not null
            ? $"'{FunctionName}' function definition statement"
            : "function definition statement";

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

    public Result SetOptionalVariables(params VariableToken[] variableTokens)
    {
        _expectedVariables = variableTokens;
        return true;
    }

    public IEnumerator<float> RunProperly(params Value[] values)
    {
        if (LineNum.HasValue)
            Script.CurrentLine = LineNum.Value;

        if (values.Length != _expectedVariables.Length)
        {
            throw new ScriptRuntimeError(this, 
                $"Provided [{values.Length}] values, but [{_expectedVariables.Length}] were expected."
            );
        }

        foreach (var (value, variableToken) in values.Zip(_expectedVariables, (v, t) => (v, t)))
        {
            if (!variableToken.ValueType.IsInstanceOfType(value))
            {
                throw new ScriptRuntimeError(this, 
                    $"Provided variable '{variableToken.Name}' of type '{value.FriendlyTypeName()}' " +
                    $"does not match expected type '{variableToken.ValueType.FriendlyTypeName()}'"
                );
            }
            
            _localVariables.Add(Variable.Create(variableToken.Name, value));
        }
        
        Script.AddVariables(_localVariables.ToArray());
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
        foreach (var coro in Children
             .TakeWhile(_ => Script.IsRunning)
             .Select(child => child.ExecuteBaseContext())
        )
        {
            while (coro.MoveNext())
            {
                if (!Script.IsRunning || _end)
                {
                    goto Exit;
                }
                
                yield return coro.Current;
            }
        }
        
        Exit:
        _localVariables.ForEach(v => Script.RemoveVariable(v));
    }
}