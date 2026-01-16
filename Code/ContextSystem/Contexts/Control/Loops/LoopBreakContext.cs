using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class LoopBreakContext : StandardContext, IKeywordContext
{
    public string KeywordName => "break";
    public string Description => "Makes a given loop (that the 'break' keyword is inside) act as it has completely " +
                                 "ended its execution (\"breaks\" free from the loop)";
    public string[] Arguments => [];

    protected override string FriendlyName => "'break' keyword";

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