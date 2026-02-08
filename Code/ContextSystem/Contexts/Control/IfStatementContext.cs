using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

public class IfStatementContext : StatementContext, IExtendableStatement, IKeywordContext
{
    public string KeywordName => "if";
    public string Description => "This statement will execute only if the provided condition is met.";
    public string[] Arguments => ["[condition]"];
    
    public IExtendableStatement.Signal Exports => IExtendableStatement.Signal.DidntExecute;
    public Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = [];

    private readonly List<BaseToken> _condition = [];
    
    private NumericExpressionReslover.CompiledExpression _expression;

    protected override string FriendlyName => "'if' statement";

    protected override TryAddTokenRes OnAddingToken(BaseToken token)
    {
        if (NumericExpressionReslover.IsValidForExpression(token).HasErrored(out var error))
        {
            return TryAddTokenRes.Error(error);
        }        
        
        _condition.Add(token);
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        if (NumericExpressionReslover.CompileExpression(_condition.ToArray())
            .HasErrored(out var error, out var cond))
        {
            return error;
        }
        
        _expression = cond;
        
        return _condition.Count > 0
            ? true
            : "An if statement expects to have a condition, but none was provided!";
    }

    protected override IEnumerator<float> Execute()
    {
        if (_expression.Evaluate().HasErrored(out var error, out var objResult))
        {
            throw new ScriptRuntimeError(this, error);
        }

        if (objResult is not bool result)
        {
            throw new ScriptRuntimeError(this, $"An if statement condition must evaluate to a boolean value, but received {objResult.FriendlyTypeName()}");
        }
        
        if (!result)
        {
            if (!RegisteredSignals.TryGetValue(IExtendableStatement.Signal.DidntExecute, out var enumerator))
            {
                yield break;
            }
            
            var didntExecuteCoro = enumerator();
            while (didntExecuteCoro.MoveNext())
            {
                yield return didntExecuteCoro.Current;
            }

            yield break;
        }
        
        var coro = RunChildren();
        while (coro.MoveNext())
        {
            yield return coro.Current;
        }
    }
}