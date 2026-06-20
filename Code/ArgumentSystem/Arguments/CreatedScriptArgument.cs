using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class CreatedScriptArgument(string name) : Argument(name)
{
    public override string InputDescription => "Name of a script to create";

    [UsedImplicitly]
    public OldDynamicTryGet<Script> GetConvertSolution(BaseToken token)
    {
        return new(() => Script.CreateByScriptName(token.BestStaticTextRepr(), null));
    }
}