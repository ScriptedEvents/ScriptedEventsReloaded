using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.Plugin;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class ForeverLoop : LoopContextWithSingleIterationVariable<NumberValue>, IKeywordContext
{
    private readonly Result _mainErr = "Cannot create 'forever' loop.";

    protected override string DetailedUsage =>
        $$"""
          # A forever loop must yield so the server can continue processing.
          forever
              wait 1m
              Print "One minute has passed."
          end

          # The optional binding contains the 1-based iteration number.
          forever with $iter
              wait 1s
              Print "current iteration: {$iter}"
          end
          """;
    public override string KeywordName => "forever";
    public override string Description => "Runs the body indefinitely. The body must yield periodically so the server can continue processing.";
    public override ContextArgument[] Arguments => [ContextArgument.Optional(
        "with $iter", "Stores the current 1-based iteration number.",
        "A number variable compatible with the loop iteration value.")];

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error(_mainErr + "'forever' loop doesn't expect any arguments.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    protected override IEnumerator<float> Execute()
    {
        ulong iteration = 0;
        while (true)
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
}
