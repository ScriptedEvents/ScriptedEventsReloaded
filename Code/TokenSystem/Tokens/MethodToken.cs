using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Structures;

namespace SER.Code.TokenSystem.Tokens;

public class MethodToken : BaseToken, IContextableToken
{
    public Method Method { get; private set; } = null!;
    
    protected override IParseResult InternalParse(Script scr)
    {
        if (MethodIndex.TryGetMethod(Slice.RawRep).HasErrored(out _, out var method))
        {
            return new Ignore();
        }

        Method = (Method)Activator.CreateInstance(method.GetType());
        Method.Script = scr;
        Method.LineNum = LineNum;
        return new Success();
    }

    public Context GetContext(Script scr)
    {
        return new MethodContext(this)
        {
            LineNum = LineNum,
            Script = scr,
        };
    }
}