using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;

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

    public override string ToString()
    {
        return $"<{Value.GetType().GetAccurateName()} reference | {Value.GetHashCode()}>";
    }
}

public class ReferenceValue<T>(T? value) : ReferenceValue(value)
{
    public new T Value => (T) base.Value;
}