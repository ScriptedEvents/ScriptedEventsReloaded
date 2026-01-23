using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.BaseMethods.Synchronous;

public abstract class LiteralValueReturningMethod : ReturningMethod<LiteralValue>
{
    public abstract TypeOfValue LiteralReturnTypes { get; }
    public sealed override TypeOfValue Returns => LiteralReturnTypes;
}