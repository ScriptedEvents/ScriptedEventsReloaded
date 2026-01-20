using System.Collections;
using LabApi.Features.Wrappers;
using SER.Code.Helpers.Exceptions;

namespace SER.Code.ValueSystem;

public abstract class Value
{
    public abstract bool EqualCondition(Value other);
    
    public abstract int HashCode { get; }

    public static Value Parse(object obj)
    {
        if (obj is null) throw new AndrzejFuckedUpException();
        if (obj is Value v) return v;
        
        return obj switch
        {     
            bool b                  => new BoolValue(b),
            byte n                  => new NumberValue(n),
            sbyte n                 => new NumberValue(n),
            short n                 => new NumberValue(n),
            ushort n                => new NumberValue(n),
            int n                   => new NumberValue(n),
            uint n                  => new NumberValue(n),
            long n                  => new NumberValue(n),
            ulong n                 => new NumberValue(n),
            float n                 => new NumberValue((decimal)n),
            double n                => new NumberValue((decimal)n),
            decimal n               => new NumberValue(n),
            string s                => new TextValue(s),
            TimeSpan t              => new DurationValue(t),
            Player p                => new PlayerValue(p),
            IEnumerable<Player> ps  => new PlayerValue(ps),
            IEnumerable e           => new CollectionValue(e),
            _                       => new ReferenceValue(obj),
        };
    }

    public static string FriendlyName(Type type) => type.Name.Replace("Value", "").ToLower();
    public string FriendlyName() => FriendlyName(GetType());

    public override string ToString()
    {
        return $"value of type {FriendlyName()}";
    }

    public override int GetHashCode() => HashCode;

    public override bool Equals(object? obj)
    {
        return this == obj;
    }

    public static bool operator ==(Value? lhs, Value? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null || lhs.GetType() != rhs.GetType()) return false;
        return lhs.EqualCondition(rhs);
    }

    public static bool operator ==(Value? lhs, object? rhs)
    {
        return rhs is Value rhsV && lhs == rhsV;
    }

    public static bool operator ==(object? lhs, Value? rhs)
    {
        return lhs is Value lhsV && lhsV == rhs;
    }

    public static bool operator !=(Value? lhs, Value? rhs)
    {
        return !(lhs == rhs);
    }

    public static bool operator !=(Value? lhs, object? rhs)
    {
        return !(lhs == rhs);
    }

    public static bool operator !=(object? lhs, Value? rhs)
    {
        return !(lhs == rhs);
    }
}