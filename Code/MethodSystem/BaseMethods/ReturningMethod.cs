using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.BaseMethods;

public abstract class ReturningMethod : SynchronousMethod 
{
    public Value? ReturnValue { get; protected set; }
    public abstract TypeOfValue Returns { get; }
}

public abstract class ReturningMethod<T> : ReturningMethod
    where T : Value
{
    public override TypeOfValue Returns => new TypeOfValue<T>();

    protected new T? ReturnValue
    {
        set => base.ReturnValue = value;
    }
}