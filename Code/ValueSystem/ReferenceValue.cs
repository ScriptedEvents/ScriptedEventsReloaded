using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem.PropertySystem;

namespace SER.Code.ValueSystem;

[UsedImplicitly]
public class ReferenceValue(object? value) : Value, IValueWithDynamicProperties
{
    [UsedImplicitly]
    public ReferenceValue() : this(null) {}
    
    public bool IsValid => value is not null;
    public object Value => value ?? throw new CustomScriptRuntimeError("Value of reference is invalid.");

    public virtual Type ReferenceType => value?.GetType() ?? typeof(object);

    public override bool Equals(Value? other)
    {
        if (other is not ReferenceValue otherP || !IsValid || !otherP.IsValid) return false;
        return Value.Equals(otherP.Value);
    }

    public override int HashCode => Value.GetHashCode();

    public override TryGet<object> ToCSharpObject(Type targetType)
    {
        if (targetType.IsInstanceOfType(Value)) return Value;
        return $"Cannot convert reference to {Value.GetType().Name} to {targetType.Name}";
    }
    
    [UsedImplicitly]
    public new static string FriendlyName => "reference value";

    public override string ToString()
    {
        return $"<{Value.GetType().AccurateName} reference | {Value.GetHashCode()}>";
    }

    public Dictionary<string, IValueWithProperties.PropInfo> Properties => 
        ReferencePropertyRegistry.GetProperties(ReferenceType);
}

[UsedImplicitly]
public class ReferenceValue<T>(T? value) : ReferenceValue(value)
{
    [UsedImplicitly]
    public ReferenceValue() : this(default) {}

    public new T Value => (T) base.Value;

    public override Type ReferenceType => typeof(T);

    public static implicit operator ReferenceValue<T>(T? value)
    {
        return new(value);
    }

    [UsedImplicitly]
    public new static string FriendlyName => $"reference to {GetFriendlyName(typeof(T))} object";
}