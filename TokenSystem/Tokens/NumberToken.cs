using SER.ScriptSystem;
using SER.ValueSystem;

namespace SER.TokenSystem.Tokens;

public class NumberToken : LiteralValueToken<NumberValue>
{
    protected override IParseResult InternalParse(Script scr)
    {
        if (decimal.TryParse(RawRep, out var value))
        {
            Value = value;
            return new Success();
        }

        if (RawRep.EndsWith("%") && decimal.TryParse(RawRep.TrimEnd('%'), out value))
        {
            Value = value / 100;
            return new Success();
        }

        return new Ignore();
    }
}