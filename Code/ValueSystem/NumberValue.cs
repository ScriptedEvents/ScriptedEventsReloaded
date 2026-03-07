using JetBrains.Annotations;

namespace SER.Code.ValueSystem;

public class NumberValue(decimal value) : LiteralValue<decimal>(value)
{
    public static implicit operator NumberValue(decimal value)
    {
        return new(value);
    }

    public static implicit operator decimal(NumberValue value)
    {
        return value.Value;
    }

    public override string StringRep => Value.ToString();
    
    [UsedImplicitly]
    public new static string FriendlyName = "number value";
}