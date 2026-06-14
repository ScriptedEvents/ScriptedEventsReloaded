using SER.Code.ScriptSystem;

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