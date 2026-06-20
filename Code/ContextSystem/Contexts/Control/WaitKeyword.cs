using MEC;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class WaitKeyword : YieldingContext, IKeywordContext
{
    private IValueToken? _durationToken;
    private Func<OldTryGet<DurationValue>>? _getDuration;

    public override string FriendlyName => $"'{KeywordName}' keyword";
    public virtual string KeywordName => "wait";

    public virtual string Description => "Halts execution of the script for a specified amount of time.";

    public virtual string[] Arguments => ["<duration>"];

    public virtual string Example => ExampleHandler.GetExample($"{KeywordName}KeywordExample") ??
                                      """
                                      # wait for 5 seconds
                                      wait 5s

                                      # Waits using a variable
                                      $duration = 10s
                                      wait $duration
                                      """;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token is IValueToken val && val.CapableOf<DurationValue>(out var get))
        {
            _durationToken = val;
            _getDuration = get;
            return TryAddTokenRes.End();
        }

        return TryAddTokenRes.Error($"'{KeywordName}' keyword expects a duration value, but received {token.RawRep}.");
    }

    public override OldResult VerifyCurrentState()
    {
        if (_durationToken == null)
        {
            return $"The duration was not provided for the '{KeywordName}' keyword.";
        }

        return true;
    }

    protected override IEnumerator<float> Execute()
    {
        var res = _getDuration!();
        if (res.HasErrored(out var error, out var duration))
        {
            throw new ScriptRuntimeError(this, error);
        }

        yield return Timing.WaitForSeconds((float)duration.Value.TotalSeconds);
    }
}