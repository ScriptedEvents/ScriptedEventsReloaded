using System.Globalization;
using SER.Code.ScriptSystem;
using ValueType = SER.Code.ValueSystem.ValueType;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public class NumberToken : ValueToken
{
    public override ValueType ValueTypes => ValueType.Number;
    
    public override bool IsConstant => true;

    protected override IParseResult InternalParse(Script scr)
    {
        if (decimal.TryParse(RawRep, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            Value = ValueSystem.Value.Number(value);
            return new Success();
        }

        if (RawRep.EndsWith("%") && decimal.TryParse(RawRep.TrimEnd('%'), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
        {
            Value = ValueSystem.Value.Number(value / 100);
            return new Success();
        }

        return new Ignore();
    }
}