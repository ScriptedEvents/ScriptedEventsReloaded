using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Structures;

namespace SER.Code.TokenSystem.Tokens;

public class FlagArgumentToken : BaseToken, IContextableToken
{
    protected override IParseResult InternalParse()
    {
        return Slice.RawRep == "--"
            ? new Success()
            : new Ignore();
    }

    public Context? GetContext(Script? scr) => null;
}