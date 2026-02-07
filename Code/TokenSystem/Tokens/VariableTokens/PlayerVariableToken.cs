using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts.VariableDefinition;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.TokenSystem.Tokens.VariableTokens;

public class PlayerVariableToken : VariableToken<PlayerVariable, PlayerValue>, ITraversableValueToken
{
    public static string Example => "@players";

    public override Context? GetContext(Script? scr)
    {
        return new PlayerVariableDefinitionContext(this)
        {
            Script = scr,
            LineNum = LineNum,
        };
    }

    public TryGet<Value[]> GetTraversableValues()
    {
        return ExactValue.OnSuccess(val => val.TryGetValues(), null);
    }

    public static PlayerVariableToken GetToken(string representaion) => BaseToken.GetToken<PlayerVariableToken>(representaion);
}