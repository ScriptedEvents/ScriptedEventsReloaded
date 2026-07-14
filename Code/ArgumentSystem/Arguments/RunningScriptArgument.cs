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
            var name = token.BestStaticTextRepr();
            var matches = Script.RunningScripts
                .Where(script => FileSystem.FileSystem.IsScriptOrFileName(script, name))
                .ToArray();
            if (matches.Length == 0)
            {
                return $"There is no running script named '{name}'";
            }

            if (matches.Length > 1)
            {
                return $"More than one section of script '{name}' is running. Use a section name such as '{name}:1'.";
            }

            return matches[0];
        });
    }
}
