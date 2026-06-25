using System.Globalization;
using SER.Code.Extensions;
using SER.Code.ResultSystem;
using SER.Code.ScriptSystem;
using ValueType = SER.Code.ValueSystem.ValueType;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public class DurationToken : ValueToken
{
    public override ValueType ValueTypes => ValueType.Duration;
    
    public override bool IsConstant => true;

    protected override IParseResult InternalParse(Script scr)
    {
        if (Parse(RawRep).HasErrored(out var error, out var value))
        {
            return new Error(error.Value);
        }

        if (value is not { } timeSpan)
        {
            return new Ignore();
        }
        
        Value = ValueSystem.Value.Duration(timeSpan);
        return new Success();
    }

    public static TryGet<TimeSpan?> Parse(string value)
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
            return "Duration cannot be negative.".AsError();
        }

        var unit = value[unitIndex..];
        return unit switch
        {
            "s" => TimeSpan.FromSeconds(valueAsDouble),
            "ms" => TimeSpan.FromMilliseconds(valueAsDouble),
            "m" => TimeSpan.FromMinutes(valueAsDouble),
            "h" => TimeSpan.FromHours(valueAsDouble),
            "d" => TimeSpan.FromDays(valueAsDouble),
            _ => $"Provided unit '{unit}' is not valid.".AsError()
        };
    }
}