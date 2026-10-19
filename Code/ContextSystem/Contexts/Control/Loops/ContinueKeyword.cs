using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class ContinueKeyword : StandardContext, IKeywordContext
{
    public override string FriendlyName => "'continue' keyword";
    
    public string KeywordName => "continue";

    public string Description =>
        "Makes a given loop (that the 'continue' keyword is inside) act as it has reached the end of its body.";

    public ContextArgument[] Arguments => [];

    public string Example =>
        """
        # number 3 will be skipped
        repeat 5 with $iteration
            if $iteration is 3
                continue
            end
            Print "iteration {$iteration}"
        end
        """;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error("The continue keyword does not expect arguments after it.");
    }

    public override Result VerifyCurrentState()
    {
        // Look for the nearest loop around this keyword. A function body is a boundary:
        // a loop outside the function is not running when the function's lines run.
        for (var parent = ParentContext; parent is not null; parent = parent.ParentContext)
        {
            switch (parent)
            {
                case LoopContext:
                    return true;
                case FuncStatement:
                    return "'continue' can only be used inside a loop ('repeat', 'over', 'while' or 'forever') " +
                           "in the same function. To leave the function early, use 'return' instead.";
            }
        }

        return "'continue' can only be used inside a loop ('repeat', 'over', 'while' or 'forever').";
    }

    protected override void Execute()
    {
        ParentContext?.SendControlMessage(new Continue());
    }
}
