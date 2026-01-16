using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class RunningScriptArgument(string name) : Argument(name)
{
    public override string InputDescription => "Name of a currently running script";

    [UsedImplicitly]
    public DynamicTryGet<Script> GetConvertSolution(BaseToken token)
    {
        return new(() =>
        {
            var name = token.GetBestTextRepresentation(Script);
            if (Script.RunningScripts.FirstOrDefault(scr => scr.Name == name) is not {} runningScript)
            {
                return $"There is no running script named '{name}'";
            }
            
            return runningScript;
        });
    }
}