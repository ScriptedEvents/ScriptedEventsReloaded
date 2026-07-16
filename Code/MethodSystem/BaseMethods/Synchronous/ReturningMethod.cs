using SER.Code.MethodSystem.BaseMethods.Interfaces;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.MethodSystem.BaseMethods.Synchronous;

public abstract class ReturningMethod : SynchronousMethod, IReturningMethod
{
    public Value? ReturnValue { get; protected set; }
    public abstract TypeOfValue Returns { get; }
}

public abstract class ReturningMethod<T> : ReturningMethod, IReturningMethod<T>
    where T : Value
{
    public override TypeOfValue Returns => new TypeOfValue<T>();

    public new T ReturnValue
    {
        get => (T)base.ReturnValue!;
        protected set => base.ReturnValue = value;
    }
}