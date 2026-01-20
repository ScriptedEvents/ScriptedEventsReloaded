using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.ContextSystem.CommunicationInterfaces;

/// <summary>
/// Marks that the context before is a statement that accepts optional variable definitions.
/// </summary>
public interface IAcceptOptionalVariableDefinitions
{
    public Result SetOptionalVariables(params VariableToken[] variableTokens);
}