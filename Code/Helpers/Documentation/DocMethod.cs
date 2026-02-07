using SER.Code.MethodSystem.BaseMethods;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.Helpers.Documentation;

public class DocMethod : DocComponent
{
}

public class DocMethod<T> : DocMethod where T : Method, new()
{
    public DocMethod(params BaseToken[] arguments)
    {
        
    }
}