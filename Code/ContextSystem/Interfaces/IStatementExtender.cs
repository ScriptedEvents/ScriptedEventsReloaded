namespace SER.Code.ContextSystem.Interfaces;

public interface IStatementExtender
{
    public abstract IExtendableStatement.Signal Extends { get; }
}