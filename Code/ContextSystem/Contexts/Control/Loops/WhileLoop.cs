using SER.Code.ContextSystem.BaseContexts;
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
    private readonly List<BaseToken> _condition = [];

    private readonly Result _rs = "Cannot create 'while' loop.";
    private NumericExpressionResolver.CompiledExpression _expression = null!;
    public override string KeywordName => "while";

    public override string Description =>
        "A loop which will execute its body as long as the provided condition is evaluated to true.";

    public override ContextArgument[] Arguments => [ContextArgument.Required(
        "$condition", "Condition checked before each iteration.", "A boolean expression."),
        ContextArgument.Optional("with $iter", "Stores the current 1-based iteration number.",
            "A number variable compatible with the loop iteration value.")];

    protected override string DetailedUsage =>
        """
        # Check the condition before every iteration.
        while {AmountOf @all} > 0
            wait 1s
            Print "there are players on the server!"
        end

        # Bind the current 1-based iteration number when needed.
        while {Chance 90%} with $iter
            Print "current attempt to leave loop: {$iter}"
            wait 1s
        end
        """;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        _condition.Add(token);
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        if (NumericExpressionResolver.CompileExpression(_condition.ToArray())
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
            using var coro = RunChildren();
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
