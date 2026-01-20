using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.BaseMethods;

public abstract class LiteralValueReturningMethod : ReturningMethod<LiteralValue>
{
    public abstract TypeOfValue LiteralReturnTypes { get; }
    public override TypeOfValue Returns => LiteralReturnTypes;
}