using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class OnErrorStatement : StatementContext, IStatementExtender, IKeywordContext, IAcceptOptionalVariableDefinitionsContext
{
    public string KeywordName => "on_error";
    public string Description =>
        $"Catches an exception thrown inside of a {typeof(AttemptStatement).FriendlyTypeName(true)}";
    
    public string[] Arguments => [];
    public string Example =>
        """
        &collection = EmptyCollection
        attempt
            Print {CollectionFetch &collection 2}
            # ERROR: there's nothing at index 2
        on_error
            with $message
            
            # this will print the error message
            Print "Error: {$message}"
        end
        """;

    public IExtendableStatement.Signal Extends => IExtendableStatement.Signal.ThrewException;
    protected override string FriendlyName => "'on_error' statement";

    public Exception? Exception
    {
        get;
        set
        {
            if (field is not null)
                return;
            field = value;
        }
    }
    private LiteralVariableToken? _variableToken;
    
    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error($"A {FriendlyName} does not expect any arguments.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    public Result SetOptionalVariables(params VariableToken[] variableTokens)
    {
        if (variableTokens.Length > 1)
            return $"Too many arguments provided for {FriendlyName}, only 1 is allowed.";

        if (variableTokens.First() is not LiteralVariableToken token)
        {
            return $"{FriendlyName} only accepts a literal variable.";
        }
        
        _variableToken = token;
        return true;
    }
    
    protected override IEnumerator<float> Execute()
    {
        Variable? variable = null;
        if (_variableToken is not null)
        {
            variable = Variable.Create(_variableToken.Name, Value.Parse(Exception!.Message, Script));
            Script.AddLocalVariable(variable);
        }

        var coro = RunChildren();
        while (coro.MoveNext())
            yield return coro.Current;
        
        if (variable is not null) Script.RemoveLocalVariable(variable);
    }
}