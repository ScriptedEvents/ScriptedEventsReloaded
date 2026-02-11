using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ContextSystem.Contexts.VariableDefinition;

public class CollectionVariableDefinitionContext(VariableToken<CollectionVariable, CollectionValue> varToken) : 
    VariableDefinitionContext<VariableToken<CollectionVariable, CollectionValue>, CollectionValue, CollectionVariable>(varToken);