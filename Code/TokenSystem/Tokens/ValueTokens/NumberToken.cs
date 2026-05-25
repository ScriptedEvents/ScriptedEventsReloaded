using System.Globalization;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public class NumberToken : LiteralValueToken<NumberValue>
{
    protected override IParseResult InternalParse(Script scr)
    {
        if (decimal.TryParse(RawRep, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            Value = value;
            return new Success();
        }

        if (RawRep.EndsWith("%") && decimal.TryParse(RawRep.TrimEnd('%'), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
        {
            Value = value / 100;
            return new Success();
        }

        return new Ignore();
    }
}