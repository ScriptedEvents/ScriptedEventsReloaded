using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.ContextSystem.Interfaces;

/// <summary>
///     Marks that the context before is a statement that accepts optional variable definitions.
/// </summary>
public interface IAcceptOptionalVariableDefinitionsContext
{
    public OldResult SetOptionalVariables(params VariableToken[] variableTokens);
}