using SER.Code.ContextSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;
using Log = SER.Code.Helpers.Log;

namespace SER.Code.ContextSystem.Contexts.VariableDefinition;

public class PlayerVariableDefinitionContext(VariableToken<PlayerVariable, PlayerValue> varToken) : 
    VariableDefinitionContext<VariableToken<PlayerVariable, PlayerValue>, PlayerValue, PlayerVariable>(varToken)
{
    protected override (TryAddTokenRes result, Func<PlayerValue> parser) AdditionalParsing(BaseToken token)
    {
        if (token is ParenthesesToken { RawContent: "" } && token.Script is not null)
        {
            Log.ScriptWarn(
                token.Script, 
                $"Using () to create an empty player variable will be removed in future versions of SER. " +
                $"Please use the @empty variable to create an empty variable instead."
            );
            
            return (TryAddTokenRes.End(), () => new([]));
        }

        return base.AdditionalParsing(token);
    }
}