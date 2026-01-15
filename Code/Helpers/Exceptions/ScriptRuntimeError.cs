using SER.Code.ContextSystem.BaseContexts;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.Helpers.Exceptions;

public class ScriptRuntimeError : SystemException
{
    protected ScriptRuntimeError(string error) : base(error)
    {
    }

    public ScriptRuntimeError(Context context, string error) : base($"{context} has errored: {error}")
    {
    }

    public ScriptRuntimeError(Method method, string error) : base($"{method} has errored: {error}")
    {
    }
}

public class CustomScriptRuntimeError(string error) : ScriptRuntimeError(error);