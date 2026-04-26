using JetBrains.Annotations;
using SER.Code.ValueSystem.PropertySystem;
using UnityEngine;
using ValueType = SER.Code.ValueSystem.Other.ValueType;

namespace SER.Code.ValueSystem;

[UsedImplicitly]
public class ColorValue(Color color) : LiteralValue<Color>(color), IValueWithProperties
{
    [UsedImplicitly]
    public ColorValue() : this(Color.white) {}

    public override string StringRep => Value.ToHex();

    [UsedImplicitly]
    public new static string FriendlyName => "color value";

    private class Prop<T>(Func<ColorValue, T> handler, string? description)
        : IValueWithProperties.PropInfo<ColorValue, T>(handler, description) where T : Value;

    public Dictionary<string, IValueWithProperties.PropInfo> Properties { get; } = new()
    {
        ["r"] = new Prop<NumberValue>(c => (decimal)c.Value.r, "Red component of the color (0-1)"),
        ["g"] = new Prop<NumberValue>(c => (decimal)c.Value.g, "Green component of the color (0-1)"),
        ["b"] = new Prop<NumberValue>(c => (decimal)c.Value.b, "Blue component of the color (0-1)"),
        ["a"] = new Prop<NumberValue>(c => (decimal)c.Value.a, "Alpha component of the color (0-1)"),
        ["hex"] = new Prop<StaticTextValue>(c => c.Value.ToHex(), "Hexadecimal representation of the color"),
        ["valType"] = new Prop<EnumValue<ValueType>>(_ => ValueType.Color, "The type of the value")
    };
    
    public static implicit operator ColorValue(Color color) => new(color);
}