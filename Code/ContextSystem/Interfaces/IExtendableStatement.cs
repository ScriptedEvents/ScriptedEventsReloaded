using JetBrains.Annotations;

namespace SER.Code.ContextSystem.Interfaces;

/// <summary>
/// Defines this context as a statement that can be extended by other statements.
/// </summary>
public interface IExtendableStatement
{
    [Flags]
    public enum Signal
    {
        [UsedImplicitly] 
        None           = 0,
        DidntExecute   = 1 << 0,
        EndedExecution = 1 << 1,
    }

    /// <summary>
    /// Defines the "signals" that the following statements can use to chain to this statement.
    /// </summary>
    public abstract Signal Exports { get; }
    
    public Dictionary<Signal, Func<IEnumerator<float>>> RegisteredSignals { get; }
}