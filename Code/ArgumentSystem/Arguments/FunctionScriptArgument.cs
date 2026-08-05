using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class FunctionScriptArgument(string name) : CreatedScriptArgument(name)
{
    [UsedImplicitly]
    public new DynamicTryGet<Script> GetConvertSolution(BaseToken token)
    {
        return new(() => Script.CreateFunctionByScriptName(token.BestStaticTextRepr(), null));
    }
}
