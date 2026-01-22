using System.Globalization;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens;

public class DurationToken : LiteralValueToken<DurationValue>
{
    protected override IParseResult InternalParse(Script scr)
    {
        var value = RawRep;
        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result) && result.TotalMilliseconds > 0)
        {
            Value = result;
            return new Success();
        }

        var unitIndex = Array.FindIndex(value.ToCharArray(), char.IsLetter);
        if (unitIndex == -1)
        {
            return new Ignore();
        }
        
        string numberString = string.Join("", value.Take(unitIndex).ToArray());
        if (!double.TryParse(numberString, NumberStyles.Any, CultureInfo.InvariantCulture, out var valueAsDouble))
        {
            return new Ignore();
        }
        
        if (valueAsDouble < 0)
        {
            return new Error("Duration cannot be negative.");
        }

        var unit = value.Substring(unitIndex);
        TimeSpan? timeSpan = unit switch
        {
            "s" => TimeSpan.FromSeconds(valueAsDouble),
            "ms" => TimeSpan.FromMilliseconds(valueAsDouble),
            "m" => TimeSpan.FromMinutes(valueAsDouble),
            "h" => TimeSpan.FromHours(valueAsDouble),
            "d" => TimeSpan.FromDays(valueAsDouble),
            _ => null
        };

        if (timeSpan is null)
        {
            return new Error($"Provided unit {unit} is not valid.");
        }

        Value = timeSpan.Value;
        return new Success();
    }
}