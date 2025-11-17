using StringBuilder = System.Text.StringBuilder;

namespace SER.ValueSystem;

public class DurationValue(TimeSpan value) : LiteralValue<TimeSpan>(value)
{
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
}