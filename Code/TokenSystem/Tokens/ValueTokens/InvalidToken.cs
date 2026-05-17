using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public class InvalidToken : LiteralValueToken<InvalidValue>
{
    protected override IParseResult InternalParse(Script scr)
    {
        if (!string.Equals(RawRep, "invalid", StringComparison.InvariantCultureIgnoreCase))
        {
            return new Ignore();
        }
        
        Value = new InvalidValue();
        return new Success();
    }
}