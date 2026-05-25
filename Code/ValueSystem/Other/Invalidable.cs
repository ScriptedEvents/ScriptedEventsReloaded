using SER.Code.Helpers.ResultSystem;

namespace SER.Code.ValueSystem.Other;

public interface IInvalidable
{
    Type WrappedType { get; }
    Value SafeValue { get; }
}

public class Invalidable<T>(T? value) : Value, IInvalidable where T : Value
{
    public Type WrappedType => typeof(T);
    public Value SafeValue => field ??= value is not null ? value : new InvalidValue();
    
    public override int HashCode => 
        SafeValue.GetHashCode();

    public override TryGet<object> ToCSharpObject(Type targetType) => 
        SafeValue.ToCSharpObject(targetType);
    
    public override bool Equals(Value? other) =>
        SafeValue.Equals(other);

    public static implicit operator Invalidable<T>(T value) => new(value);
    public static implicit operator Invalidable<T>(InvalidValue _) => new(null);
    
    [UsedImplicitly]
    public new static string FriendlyName => $"{GetFriendlyName(typeof(T))} (may be invalid)";
}