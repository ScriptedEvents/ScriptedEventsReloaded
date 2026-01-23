using SER.Code.MethodSystem.BaseMethods.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.BaseMethods.Yielding;

public abstract class YieldingReturningMethod : YieldingMethod, IReturningMethod 
{
    public Value? ReturnValue { get; protected set; }
    public abstract TypeOfValue Returns { get; }
}

public abstract class YieldingReturningMethod<T> : YieldingReturningMethod, IReturningMethod<T>
    where T : Value
{
    public override TypeOfValue Returns => new TypeOfValue<T>();

    public new T ReturnValue
    {
        get => (T)base.ReturnValue!;
        protected set => base.ReturnValue = value;
    }
}