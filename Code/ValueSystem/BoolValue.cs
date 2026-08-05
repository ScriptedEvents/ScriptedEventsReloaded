using SER.Code.ValueSystem.PropertySystem;
using ValueType = SER.Code.ValueSystem.Other.ValueType;

namespace SER.Code.ValueSystem;

public class BoolValue(bool value) : LiteralValue<bool>(value), IValueWithProperties
{
    [UsedImplicitly]
    public BoolValue() : this(false) {}

    public static implicit operator BoolValue(bool value)
    {
        return new(value);
    }
    
    public static implicit operator bool(BoolValue value)
    {
        return value.Value;
    }

    public override string StringRep => Value.ToString().ToLowerInvariant();

    [UsedImplicitly]
    public new static string FriendlyName => "bool value";

    private class Prop<T>(Func<BoolValue, T> handler, string? description)
        : IValueWithProperties.PropInfo<BoolValue, T>(handler, description) where T : Value;

    private static Dictionary<string, IValueWithProperties.PropInfo>? _properties;
    public Dictionary<string, IValueWithProperties.PropInfo> Properties => _properties ??= new()
    {
        ["not"] = new Prop<BoolValue>(b => !b.Value, "Inverted boolean value"),
        ["asNumber"] = new Prop<NumberValue>(b => b.Value ? 1m : 0m, "Converts boolean to number (1 for true, 0 for false)"),
        ["asText"] = new Prop<StaticTextValue>(b => b.Value.ToString().ToLowerInvariant(), "Converts boolean to text ('true' or 'false')"),
        ["valType"] = new Prop<EnumValue<ValueType>>(_ => ValueType.Bool, "The type of the value")
    };
}