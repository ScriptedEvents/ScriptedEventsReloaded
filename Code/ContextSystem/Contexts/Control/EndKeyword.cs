using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class EndKeyword : StandardContext, IKeywordContext
{
    public override string FriendlyName => "'end' keyword";
    public string KeywordName => "end";
    public string Description => "Ends the current statement's body.";
    public ContextArgument[] Arguments => [];
    public string Example =>
        """
        if {@sender -> role} is "ClassD"
            Print "This block ends here."
        end
        """;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public override TryAddTokenRes TryAddToken(BaseToken token)
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
