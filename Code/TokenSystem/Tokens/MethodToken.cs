using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Structures;

namespace SER.Code.TokenSystem.Tokens;

public class MethodToken : BaseToken, IContextableToken
{
    public Method MethodInstance { get; private set; } = null!;
    
    protected override IParseResult InternalParse()
    {
        if (MethodIndex.TryGetMethod(Slice.RawRep).HasErrored(out _, out var method))
        {
            return new Ignore();
        }

        MethodInstance = (Method)Activator.CreateInstance(method.GetType());
        MethodInstance.Script = Script!;
        MethodInstance.LineNum = LineNum;
        return new Success();
    }

    public Context? GetContext(Script? scr)
    {
        return new MethodContext(this)
        {
            LineNum = LineNum,
            Script = scr!,
        };
    }
}