using SER.Helpers.Exceptions;
using SER.Helpers.Extensions;

namespace SER.ValueSystem;

public class ReferenceValue(object? value) : Value
{
    public bool IsValid => value is not null;
    public object Value => value ?? throw new ScriptRuntimeError("Value of reference is invalid.");

    public override string ToString()
    {
        return $"<{Value.GetType().GetAccurateName()} reference>";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ReferenceValue other) return false;
        return Equals(other);
    }

    protected bool Equals(ReferenceValue other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}