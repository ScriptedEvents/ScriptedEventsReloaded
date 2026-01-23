using SER.Code.MethodSystem.BaseMethods.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.BaseMethods.Synchronous;

/// <summary>
/// Represents a method that returns a reference to an object that cannot be represented fully in text form (not counting players.)
/// </summary>
public abstract class ReferenceReturningMethod : ReturningMethod<ReferenceValue>, IReferenceReturningMethod
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