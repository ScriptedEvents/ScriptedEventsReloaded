using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class ContinueKeyword : StandardContext, IKeywordContext
{
    public string KeywordName => "continue";
    
    public string Description =>
        "Makes a given loop (that the 'continue' keyword is inside) act as it has reached the end of its body.";
    
    public string[] Arguments => [];
    
    public string? Example => null;

    protected override string FriendlyName => "'continue' keyword";

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error("The continue keyword does not expect arguments after it.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    protected override void Execute()
    {
        ParentContext?.SendControlMessage(new Continue());
    }
}