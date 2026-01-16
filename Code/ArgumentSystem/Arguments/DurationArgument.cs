using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class DurationArgument(string name) : Argument(name)
{
    public override string InputDescription => "Duration in format #ms (milliseconds), #s (seconds), #m (minutes) etc., e.g. 5s or 2m";

    [UsedImplicitly]
    public DynamicTryGet<TimeSpan> GetConvertSolution(BaseToken token)
    {
        Result rs = $"Value '{token.RawRep}' is not a duration.";
        if (token is not IValueToken valueToken || !valueToken.CanReturn<DurationValue>(out var get))
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