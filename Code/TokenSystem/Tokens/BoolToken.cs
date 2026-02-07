using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens;

public class BoolToken : LiteralValueToken<BoolValue>
{
    protected override IParseResult InternalParse()
    {
        if (bool.TryParse(Slice.RawRep, out var res1))
        {
            Value = res1;
            return new Success();
        }
        
        return new Ignore();
    }
}