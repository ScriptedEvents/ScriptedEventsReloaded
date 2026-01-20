using JetBrains.Annotations;

namespace SER.Code.ContextSystem.Interfaces;

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

    public abstract Signal AllowedSignals { get; }
    public Dictionary<Signal, Func<IEnumerator<float>>> RegisteredSignals { get; }
}