using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class WhileLoopContext : LoopContextWithSingleIterationVariable<NumberValue>, IExtendableStatement
{
    private readonly Result _rs = "Cannot create 'while' loop.";
    private readonly List<BaseToken> _condition = []; 
    private NumericExpressionReslover.CompiledExpression _expression;
    
    public override string KeywordName => "while";
    public override string Description =>
        "A statement which will execute its body as long as the provided condition is evaluated to true.";
    public override string[] Arguments => ["[condition...]"];

    public override Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = [];

    protected override string FriendlyName => "'while' loop statement";

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
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
            _rs + "The condition was not provided.");
    }

    protected override IEnumerator<float> Execute()
    {
        ulong iteration = 0;
        while (GetExpressionResult())
        {
            SetVariable(++iteration);
            var coro = RunChildren();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }
            
            RemoveVariable();
            if (ReceivedBreak) break;
        }

        if (RegisteredSignals.TryGetValue(IExtendableStatement.Signal.EndedExecution, out var coroFunc))
        {
            var coro = coroFunc();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }
        }
    }

    private bool GetExpressionResult()
    {
        if (_expression.Evaluate().HasErrored(out var error, out var objResult))
        {
            throw new ScriptRuntimeError(this, error);
        }

        if (objResult is not bool result)
        {
            throw new ScriptRuntimeError(this, $"A while statement condition must evaluate to a boolean value, but received {objResult.FriendlyTypeName()}");
        }

        return result;
    }
}