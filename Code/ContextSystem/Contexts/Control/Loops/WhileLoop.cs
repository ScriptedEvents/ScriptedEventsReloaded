using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class WhileLoop : LoopContextWithSingleIterationVariable<NumberValue>
{
    public override string KeywordName => "while";
    
    public override string Description =>
        "A loop which will execute its body as long as the provided condition is evaluated to true.";
    
    public override string[] Arguments => ["[condition...]"];

    protected override string Usage =>
        """
        # while loop repeats its body while the provided condition is met
        while {AmountOf @all} > 0
            Wait 1s
            Print "there are players on the server!"
        end

        # ========================================
        # you may also use a "with" keyword to define an iteration variable
        while {Chance 90%}
            with $iter
            
            Print "current attempt to leave loop: {$iter}"
            Wait 1s
        end
        """;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    private readonly Result _rs = "Cannot create 'while' loop.";
    private readonly List<BaseToken> _condition = []; 
    private NumericExpressionReslover.CompiledExpression _expression;
    
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
    }

    private bool GetExpressionResult()
    {
        if (_expression.Evaluate().HasErrored(out var error, out var objResult))
        {
            throw new ScriptRuntimeError(this, error);
        }

        if (objResult is not bool result)
        {
            throw new ScriptRuntimeError(
                this, 
                $"A while statement condition must evaluate to a boolean value, " +
                $"but received {objResult.FriendlyTypeName()}"
            );
        }

        return result;
    }
}