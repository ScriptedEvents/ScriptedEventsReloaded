using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class BreakKeyword : StandardContext, IKeywordContext
{
    public override string FriendlyName => "'break' keyword";
    public string KeywordName => "break";
    public string Description =>
        "Makes a given loop or function (that the 'break' keyword is inside) act as it has completely ended its execution " +
        "(\"breaks\" free from the loop/function)";

    public ContextArgument[] Arguments => [];

    public string Example =>
        """
        # the execution will "break" free after the third iteration
        repeat 10 with $iteration
            if {$iteration is 3}
                break
            end
            Print "iteration {$iteration}"
        end
        """;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error("The 'break' keyword does not expect arguments after it.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    protected override void Execute()
    {
        ParentContext?.SendControlMessage(new Break());
    }
}
