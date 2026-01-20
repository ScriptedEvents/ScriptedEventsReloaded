using SER.Code.ContextSystem.BaseContexts;
using SER.Code.Helpers.ResultSystem;

namespace SER.Code.ContextSystem.CommunicationInterfaces;

/// <summary>
/// Requests the previous statement context to be provided
/// </summary>
public interface IRequireCurrentStatement
{
    public Result AcceptStatement(StatementContext context);
}