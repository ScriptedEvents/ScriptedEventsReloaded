using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ContextSystem.Contexts.VariableDefinition;

public class LiteralVariableDefinitionContext(VariableToken<LiteralVariable, LiteralValue> varToken) :
    VariableDefinitionContext<VariableToken<LiteralVariable, LiteralValue>, LiteralValue, LiteralVariable>(varToken);