using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.Helpers.Documentation;

public abstract class DocMethod : DocComponent;

public class DocMethod<T> : DocMethod where T : Method, new()
{
    private readonly string _rep;
    public DocMethod(bool isExpression, params BaseToken[] arguments)
    {
        _rep = $"{Method.GetFriendlyName(typeof(T))} {string.Join(" ", arguments.Select(a => a.RawRep))}";
        if (Script.VerifyContent(_rep).HasErrored(out var error))
        {
            throw new InvalidOperationException(error);
        }
        
        if (isExpression)
        {
            _rep = $"{{{_rep}}}";
        }
    }

    public override string ToString() => _rep;
}