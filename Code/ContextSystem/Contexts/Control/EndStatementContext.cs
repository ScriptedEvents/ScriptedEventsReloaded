using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

public class EndStatementContext : StandardContext, IKeywordContext
{
    public string KeywordName => "end";
    public string Description => "Ends the current statement's body.";
    public string[] Arguments => [];

    protected override string FriendlyName => "'end' keyword";

    protected override TryAddTokenRes OnAddingToken(BaseToken token)
    {
        return TryAddTokenRes.Error("There can't be anything else on the same line as the 'end' keyword.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    protected override void Execute()
    {
    }
}