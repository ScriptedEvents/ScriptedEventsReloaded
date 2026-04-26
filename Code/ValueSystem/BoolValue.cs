using JetBrains.Annotations;
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

    public override string StringRep => Value.ToString().ToLower();

    [UsedImplicitly]
    public new static string FriendlyName => "bool value";

    private class Prop<T>(Func<BoolValue, T> handler, string? description)
        : IValueWithProperties.PropInfo<BoolValue, T>(handler, description) where T : Value;

    public Dictionary<string, IValueWithProperties.PropInfo> Properties { get; } = new()
    {
        ["not"] = new Prop<BoolValue>(b => !b.Value, "Inverted boolean value"),
        ["asNumber"] = new Prop<NumberValue>(b => b.Value ? 1m : 0m, "Converts boolean to number (1 for true, 0 for false)"),
        ["asString"] = new Prop<StaticTextValue>(b => b.Value.ToString().ToLower(), "Converts boolean to string ('true' or 'false')"),
        ["valType"] = new Prop<EnumValue<ValueType>>(_ => ValueType.Bool, "The type of the value")
    };
}