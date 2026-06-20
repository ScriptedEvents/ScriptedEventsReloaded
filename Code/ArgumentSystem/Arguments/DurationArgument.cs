using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;

namespace SER.Code.ArgumentSystem.Arguments;

public class DurationArgument(string name) : Argument(name)
{
    public override string InputDescription => "Duration in format #ms (milliseconds), #s (seconds), #m (minutes) etc., e.g. 5s or 2m";

    [UsedImplicitly]
    public OldDynamicTryGet<TimeSpan> GetConvertSolution(BaseToken token)
    {
        OldResult rs = $"Value '{token.RawRep}' is not a duration.";
        if (token is not IValueToken valueToken || !valueToken.CapableOf<DurationValue>(out var get))
        {
            return rs;
        }

        if (valueToken.IsConstant)
        {
            return get().OnSuccess(v => v.Value, rs);
        }

        return new(() => get().OnSuccess(v => v.Value, rs));
    }
}