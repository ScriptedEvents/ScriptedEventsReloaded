using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ContextSystem.Contexts.VariableDefinition;

public class LiteralVariableDefinitionContext :
    VariableDefinitionContext<VariableToken<LiteralVariable, LiteralValue>, LiteralValue, LiteralVariable>
{
    public LiteralVariableDefinitionContext(VariableToken<LiteralVariable, LiteralValue> varToken) : base(varToken)
    {
        AdditionalTokenParser = token =>
        {
            if (token is TextToken textToken)
            {
                return () => textToken.Value;
            }

            return null;
        };
    }
}


