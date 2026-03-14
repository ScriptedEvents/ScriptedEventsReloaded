using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public class BoolToken : LiteralValueToken<BoolValue>
{
    protected override IParseResult InternalParse(Script scr)
    {
        if (bool.TryParse(Slice.RawRep, out var res1))
        {
            Value = res1;
            return new Success();
        }
        
        return new Ignore();
    }
}