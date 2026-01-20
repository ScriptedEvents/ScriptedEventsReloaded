using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts.VariableDefinition;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.TokenSystem.Tokens.VariableTokens;

public class PlayerVariableToken : VariableToken<PlayerVariable, PlayerValue>
{
    public static string Example => "@players";

    public override Context GetContext(Script scr)
    {
        return new PlayerVariableDefinitionContext(this)
        {
            Script = scr,
            LineNum = LineNum,
        };
    }
}