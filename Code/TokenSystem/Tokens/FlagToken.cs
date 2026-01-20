using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Structures;

namespace SER.Code.TokenSystem.Tokens;

public class FlagToken : BaseToken, IContextableToken
{
    protected override IParseResult InternalParse(Script scr)
    {
        return Slice.RawRep == "!--"
            ? new Success()
            : new Ignore();
    }

    public Context GetContext(Script scr)
    {
        return new NoOperationContext
        {
            Script = Script,
            LineNum = LineNum,
        };
    }
}