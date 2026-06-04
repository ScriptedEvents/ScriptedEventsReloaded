using SER.Code.ScriptSystem;

namespace SER.Code.TokenSystem.Tokens;

public class AllToken : BaseToken
{
    protected override IParseResult InternalParse(Script scr)
    {
        if (string.Equals(RawRep, "all", StringComparison.InvariantCultureIgnoreCase))
        {
            return new Success();
        }

        return new Ignore();
    }
}