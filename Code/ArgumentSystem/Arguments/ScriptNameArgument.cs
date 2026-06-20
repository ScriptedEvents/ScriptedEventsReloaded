using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class ScriptNameArgument(string name) : Argument(name)
{
    public override string InputDescription => "Name of a script";

    [UsedImplicitly]
    public OldDynamicTryGet<ScriptName> GetConvertSolution(BaseToken token)
    {
        if (token.BestTextRepr().IsStatic(out var name, out var func))
        {
            return ScriptName.Create(name);
        }

        return new(() => ScriptName.Create(func()));
    }
}