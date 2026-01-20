using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts;

public class NoOperationContext : StandardContext, INotRunningContext
{
    protected override string FriendlyName => "no operation";

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    protected override void Execute()
    {
    }
}