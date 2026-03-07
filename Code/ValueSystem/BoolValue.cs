using JetBrains.Annotations;

namespace SER.Code.ValueSystem;

public class BoolValue(bool value) : LiteralValue<bool>(value)
{
    public static implicit operator BoolValue(bool value)
    {
        return new(value);
    }
    
    public static implicit operator bool(BoolValue value)
    {
        return value.Value;
    }

    public override string StringRep => Value.ToString().ToLower();

    [UsedImplicitly]
    public new static string FriendlyName = "boolean (true/false) value";
}