using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Structures;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts;

public class FlagContext : StandardContext
{
    private string? _flagName;

    public override string FriendlyName => "flag declaration";

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (Flag.FlagInfos.Count == 0)
        {
            Flag.RegisterFlags();
        }

        if (!Flag.FlagInfos.ContainsKey(token.RawRep))
        {
            return TryAddTokenRes.Error($"Flag '{token.RawRep}' is not a valid flag.");
        }

        _flagName = token.RawRep;
        return TryAddTokenRes.End();
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(_flagName is not null, "Name of the flag is missing.");
    }

    protected override void Execute()
    {
    }
}
