namespace SER.Code.ContextSystem.Interfaces;

/// <summary>
/// Defines this context as a statement that can be chained to another statement.
/// </summary>
public interface IStatementExtender
{
    /// <summary>
    /// Declares which "signal" the previous statement must define in order to be chained to that statement.
    /// </summary>
    public abstract IExtendableStatement.Signal ListensTo { get; }
}