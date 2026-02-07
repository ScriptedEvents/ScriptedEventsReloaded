using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Interfaces;

/// <summary>
/// Marks that this context is a statement that accepts optional variable definitions.
/// </summary>
public interface IAcceptOptionalVariableDefinitions
{
    /// <summary>
    /// Defines the variables for the statement.
    /// </summary>
    /// <param name="variableTokens">The variables that have been defined.</param>
    /// <returns>A result defining if the provided variables have been accepted as valid.</returns>
    public Result SetOptionalVariables(params VariableToken[] variableTokens);
    
    public TypeOfValue[]? OptionalVariableTypes { get; }
}