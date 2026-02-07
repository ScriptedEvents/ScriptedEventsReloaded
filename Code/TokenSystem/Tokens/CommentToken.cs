using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Structures;

namespace SER.Code.TokenSystem.Tokens;

public class CommentToken : BaseToken, IContextableToken
{
    protected override IParseResult InternalParse()
    {
        return RawRep.FirstOrDefault() == '#' 
            ? new Success() 
            : new Ignore();
    }

    public Context? GetContext(Script? scr) => null;
}