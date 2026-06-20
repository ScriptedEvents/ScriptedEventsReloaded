using SER.Code.ContextSystem.BaseContexts;
using SER.Code.Helpers.OldResultSystem;

namespace SER.Code.ContextSystem.Interfaces;

/// <summary>
///     Requests the previous statement context to be provided
/// </summary>
public interface IRequirePreviousStatementContext
{
    public OldResult AcceptStatement(StatementContext context);
}