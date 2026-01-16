using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class CreatedScriptArgument(string name) : Argument(name)
{
    public override string InputDescription => "Name of a script to create";
    
    [UsedImplicitly]
    public DynamicTryGet<Script> GetConvertSolution(BaseToken token)
    {
        return new(() => Script.CreateByScriptName(token.GetBestTextRepresentation(Script), null));
    }
}