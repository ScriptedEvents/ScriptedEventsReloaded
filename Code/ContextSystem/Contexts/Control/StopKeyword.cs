using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class StopKeyword : StandardContext, IKeywordContext
{
    public string KeywordName => "stop";

    public string Description => "Stops the script from executing.";

    public string[] Arguments => [];
    
    public string? Example => null;

    protected override string FriendlyName => "'stop' keyword";
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error(
            "'stop' keyword is not expecting any arguments after it.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    protected override void Execute()
    {
        Script.Stop(true);
    }
}