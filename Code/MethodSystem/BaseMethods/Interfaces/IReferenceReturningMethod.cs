namespace SER.Code.MethodSystem.BaseMethods.Interfaces;

public interface IReferenceReturningMethod
{
    public Type ReturnType { get; }
}

public interface IReferenceReturningMethod<out T> : IReferenceReturningMethod
{
    public new T ReturnType { get; }
}