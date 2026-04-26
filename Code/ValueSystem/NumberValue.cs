using JetBrains.Annotations;
using SER.Code.ValueSystem.PropertySystem;
using ValueType = SER.Code.ValueSystem.Other.ValueType;

namespace SER.Code.ValueSystem;

public class NumberValue(decimal value) : LiteralValue<decimal>(value), IValueWithProperties
{
    [UsedImplicitly]
    public NumberValue() : this(0m) {}

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
    public new static string FriendlyName => "number value";

    private class Prop<T>(Func<NumberValue, T> handler, string? description)
        : IValueWithProperties.PropInfo<NumberValue, T>(handler, description) where T : Value;

    public Dictionary<string, IValueWithProperties.PropInfo> Properties { get; } = new()
    {
        ["abs"] = new Prop<NumberValue>(n => Math.Abs(n.Value), "Absolute value of the number"),
        ["round"] = new Prop<NumberValue>(n => Math.Round(n.Value), "Rounded value of the number"),
        ["floor"] = new Prop<NumberValue>(n => Math.Floor(n.Value), "Floor value of the number"),
        ["ceil"] = new Prop<NumberValue>(n => Math.Ceiling(n.Value), "Ceiling value of the number"),
        ["isEven"] = new Prop<BoolValue>(n => n.Value % 2 == 0, "Whether the number is even"),
        ["isOdd"] = new Prop<BoolValue>(n => n.Value % 2 != 0, "Whether the number is odd"),
        ["sign"] = new Prop<NumberValue>(n => (decimal)Math.Sign(n.Value), "Sign of the number (-1, 0, or 1)"),
        ["valType"] = new Prop<EnumValue<ValueType>>(_ => ValueType.Number, "The type of the value")
    };
}