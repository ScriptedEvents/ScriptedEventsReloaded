using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.BaseMethods.Interfaces;

public interface IReturningMethod
{
    public Value? ReturnValue { get; }
    public abstract TypeOfValue Returns { get; }
}

public interface IReturningMethod<out T> : IReturningMethod where T : Value
{
    public new T ReturnValue { get; }
}