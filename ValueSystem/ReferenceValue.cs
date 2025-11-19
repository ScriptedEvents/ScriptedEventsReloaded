using SER.Helpers.Exceptions;
using SER.Helpers.Extensions;

namespace SER.ValueSystem;

public class ReferenceValue(object? value) : Value
{
    public bool IsValid => value is not null;
    public object Value => value ?? throw new ScriptRuntimeError("Value of reference is invalid.");

    public override string ToString()
    {
        return $"<{Value.GetType().GetAccurateName()} reference | {Value.GetHashCode()}>";
    }
}