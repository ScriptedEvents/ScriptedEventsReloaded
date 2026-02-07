using SER.Code.ContextSystem.BaseContexts;
using SER.Code.Helpers.ResultSystem;

namespace SER.Code.ContextSystem.Interfaces;

/// <summary>
/// Requests the previous statement context to be provided for internal verification.
/// </summary>
public interface IRequireCurrentStatement
{
    public Result AcceptStatement(StatementContext context);
}