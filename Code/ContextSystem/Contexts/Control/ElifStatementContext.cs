using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class ElifStatementContext : StatementContext, IStatementExtender, IExtendableStatement, IKeywordContext
{
    public string KeywordName => "elif";
    public string Description =>
        "If the statement above it didn't execute, 'elif' statement will try to execute if the provided condition is met.";
    public string[] Arguments => ["[condition]"];
    
    public string ExampleUsage { get; }

    public IExtendableStatement.Signal ListensTo => IExtendableStatement.Signal.DidntExecute;
    
    public IExtendableStatement.Signal Exports => IExtendableStatement.Signal.DidntExecute;
    
    public Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = new();

    private readonly List<BaseToken> _condition = [];
    
    private NumericExpressionReslover.CompiledExpression _expression;

    protected override string FriendlyName => "'elif' statement";

    public override TryAddTokenRes OnAddingToken(BaseToken token)
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

        return Result.Assert(
            _condition.Count > 0,
            "An elif statement expects to have a condition, but none was provided!"
        );
    }

    protected override IEnumerator<float> Execute()
    {
        if (_expression.Evaluate().HasErrored(out var error, out var objResult))
        {
            throw new ScriptRuntimeError(this, error);
        }

        if (objResult is not bool result)
        {
            throw new ScriptRuntimeError(this, $"An elif statement condition must evaluate to a boolean value, but received {objResult.FriendlyTypeName()}");
        }
        
        if (!result)
        {
            if (!RegisteredSignals.TryGetValue(IExtendableStatement.Signal.DidntExecute, out var enumerator))
            {
                yield break;
            }
            
            var coro = enumerator();
            while (coro.MoveNext())
            {
                if (!Script.IsRunning)
                {
                    yield break;
                }
                
                yield return coro.Current;
            }
            
            yield break;
        }
        
        foreach (var coro in Children
                     .TakeWhile(_ => Script.IsRunning)
                     .Select(child => child.ExecuteBaseContext()))
        {
            while (coro.MoveNext())
            {
                if (!Script.IsRunning)
                {
                    yield break;
                }
                
                yield return coro.Current;
            }
        }
    }
}