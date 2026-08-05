using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class RepeatLoop : LoopContextWithSingleIterationVariable<NumberValue>
{
    private readonly Result _rs = "Cannot create 'repeat' loop.";
    private Func<TryGet<ulong>>? _repeatCountExpression = null;
    
    public override string KeywordName => "repeat";
    public override string Description => "Repeats everything inside its body a given amount of times.";
    public override ContextArgument[] Arguments => [ContextArgument.Required(
        "$number", "Number of times the body should run.", "A non-negative whole-number expression."),
        ContextArgument.Optional("with $iter", "Stores the current 1-based iteration number.",
            "A number variable compatible with the loop iteration value.")];
    
    protected override string DetailedUsage =>
        """
        # Repeat a fixed number of times.
        repeat 10
            Print "hi"
        end

        # The count can be an expression or variable.
        repeat {Random 1 10 int}
            Print "hi"
        end

        # Bind the current 1-based iteration number.
        repeat 10 with $iter
            Print "current iteration: {$iter}"
        end
        """;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token is not IValueToken valToken || !valToken.CapableOf<NumberValue>(out var getNumber))
        {
            return TryAddTokenRes.Error($"Value '{token.RawRep}' cannot be interpreted as a number.");
        }

        _repeatCountExpression = () =>
        {
            if (getNumber().HasErrored(out var error, out var value))
            {
                return error;
            }

            if (value.Value < 0)
            {
                return $"Value '{value}' cannot be negative.";
            }

            return (uint)value.Value;
        };

        return TryAddTokenRes.End();
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _repeatCountExpression != null,
            _rs + "The amount of times to repeat was not provided."
        );
    }

    protected override IEnumerator<float> Execute()
    {
        if (_repeatCountExpression == null)
            throw new CoreInvariantException("Repeat context has no amount specified");

        if (_repeatCountExpression().HasErrored(out var error, out var val))
        {
            throw new ScriptRuntimeError(this, error);
        }

        for (ulong i = 0; i < val; i++)
        {
            SetVariable(i + 1);
            using var coro = RunChildren();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }

            RemoveVariable();
            if (ReceivedBreak) break;
        }
    }
}
