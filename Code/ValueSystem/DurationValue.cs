using System.Text;
using JetBrains.Annotations;
using SER.Code.ValueSystem.PropertySystem;
using ValueType = SER.Code.ValueSystem.Other.ValueType;

namespace SER.Code.ValueSystem;

public class DurationValue(TimeSpan value) : LiteralValue<TimeSpan>(value), IValueWithProperties
{
    [UsedImplicitly]
    public DurationValue() : this(TimeSpan.Zero) {}

    public static implicit operator DurationValue(TimeSpan value)
    {
        return new(value);
    }
    
    public static implicit operator TimeSpan(DurationValue value)
    {
        return value.Value;
    }

    public override string StringRep
    {
        get
        {
            StringBuilder sb = new();
            if (Value.Hours > 0)
            {
                sb.Append($"{Value.Hours}h ");
            }

            if (Value.Minutes > 0)
            {
                sb.Append($"{Value.Minutes}m ");
            }

            if (Value.Seconds > 0)
            {
                sb.Append($"{Value.Seconds}s ");
            }

            if (Value.Milliseconds > 0)
            {
                sb.Append($"{Value.Milliseconds:D3}ms ");
            }

            if (sb.Length == 0)
            {
                sb.Append("0s ");
            }
            
            return sb.Remove(sb.Length - 1, 1).ToString();
        }
    }

    [UsedImplicitly]
    public new static string FriendlyName => "duration value";

    private class Prop<T>(Func<DurationValue, T> handler, string? description)
        : IValueWithProperties.PropInfo<DurationValue, T>(handler, description) where T : Value;

    public Dictionary<string, IValueWithProperties.PropInfo> Properties { get; } = new()
    {
        ["hours"] = new Prop<NumberValue>(d => d.Value.Hours, "Hours component of the duration"),
        ["minutes"] = new Prop<NumberValue>(d => d.Value.Minutes, "Minutes component of the duration"),
        ["seconds"] = new Prop<NumberValue>(d => d.Value.Seconds, "Seconds component of the duration"),
        ["milliseconds"] = new Prop<NumberValue>(d => d.Value.Milliseconds, "Milliseconds component of the duration"),
        ["totalHours"] = new Prop<NumberValue>(d => (decimal)d.Value.TotalHours, "Total hours in the duration"),
        ["totalMinutes"] = new Prop<NumberValue>(d => (decimal)d.Value.TotalMinutes, "Total minutes in the duration"),
        ["totalSeconds"] = new Prop<NumberValue>(d => (decimal)d.Value.TotalSeconds, "Total seconds in the duration"),
        ["totalMilliseconds"] = new Prop<NumberValue>(d => (decimal)d.Value.TotalMilliseconds, "Total milliseconds in the duration"),
        ["valType"] = new Prop<EnumValue<ValueType>>(_ => ValueType.Duration, "The type of the value")
    };
}