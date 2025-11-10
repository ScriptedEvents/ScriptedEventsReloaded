using SER.ValueSystem;

namespace SER.MethodSystem.BaseMethods;

/// <summary>
/// Represents a method that returns a reference to an object that cannot be represented fully in text form (not counting players.)
/// </summary>
public abstract class ReferenceReturningMethod : ReturningMethod<ReferenceValue>
{
    public abstract Type ReturnType { get; }
}

public abstract class ReferenceReturningMethod<T> : ReferenceReturningMethod
{
    public override Type ReturnType => typeof(T);

    protected new T ReturnValue
    {
        set => base.ReturnValue = new ReferenceValue(value);
    }
}