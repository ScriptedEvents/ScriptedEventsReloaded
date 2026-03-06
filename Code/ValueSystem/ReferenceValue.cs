using JetBrains.Annotations;
using SER.Code.Exceptions;
using SER.Code.Extensions;

namespace SER.Code.ValueSystem;

public class ReferenceValue(object? value) : Value
{
    public bool IsValid => value is not null;
    public object Value => value ?? throw new CustomScriptRuntimeError("Value of reference is invalid.");

    public override bool EqualCondition(Value other)
    {
        if (other is not ReferenceValue otherP || !IsValid || !otherP.IsValid) return false;
        return Value.Equals(otherP.Value);
    }

    public override int HashCode => Value.GetHashCode();

    [UsedImplicitly]
    public new static string FriendlyName => "reference value";

    public override string ToString()
    {
        return $"<{Value.GetType().AccurateName} reference | {Value.GetHashCode()}>";
    }
}

public class ReferenceValue<T>(T? value) : ReferenceValue(value)
{
    public new T Value => (T) base.Value;

    [UsedImplicitly]
    public new static string FriendlyName => $"reference to {typeof(T).AccurateName} object";
}