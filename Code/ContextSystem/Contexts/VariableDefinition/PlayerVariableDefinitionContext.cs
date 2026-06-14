using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ContextSystem.Contexts.VariableDefinition;

public class PlayerVariableDefinitionContext(VariableToken<PlayerVariable, PlayerValue> varToken) :
    VariableDefinitionContext<VariableToken<PlayerVariable, PlayerValue>, PlayerValue, PlayerVariable>(varToken);