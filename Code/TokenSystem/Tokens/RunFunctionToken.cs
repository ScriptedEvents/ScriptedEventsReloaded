using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Structures;

namespace SER.Code.TokenSystem.Tokens;

public class RunFunctionToken : BaseToken, IContextableToken
{
    public Context GetContext(Script scr)
    {
        return new RunFunctionContext
        {
            LineNum = LineNum,
            Script = scr
        };
    }

    protected override IParseResult InternalParse(Script scr)
    {
        if (RawRep != "run")
        {
            return new Ignore();
        }
        
        return new Success();
    }
}