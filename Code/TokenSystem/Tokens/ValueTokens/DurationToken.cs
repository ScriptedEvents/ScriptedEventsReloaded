using System.Globalization;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public class DurationToken : LiteralValueToken<DurationValue>
{
    protected override IParseResult InternalParse(Script scr)
    {
        if (Parse(RawRep) is not {} timeSpan)
        {
            return new Ignore();
        }

        Value = timeSpan;
        return new Success();
    }

    public static TimeSpan? Parse(string value)
    {
        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result) && result.TotalMilliseconds > 0)
        {
            return result;
        }

        var unitIndex = Array.FindIndex(value.ToCharArray(), char.IsLetter);
        if (unitIndex == -1)
        {
            return null;
        }

        string numberString = string.Join("", value.Take(unitIndex).ToArray());
        if (!double.TryParse(numberString, NumberStyles.Any, CultureInfo.InvariantCulture, out var valueAsDouble))
        {
            return null;
        }

        if (valueAsDouble < 0)
        {
            return null;
        }

        var unit = value[unitIndex..];
        return unit switch
        {
            "s" => TimeSpan.FromSeconds(valueAsDouble),
            "ms" => TimeSpan.FromMilliseconds(valueAsDouble),
            "m" => TimeSpan.FromMinutes(valueAsDouble),
            "h" => TimeSpan.FromHours(valueAsDouble),
            "d" => TimeSpan.FromDays(valueAsDouble),
            _ => null
        };
    }
}
