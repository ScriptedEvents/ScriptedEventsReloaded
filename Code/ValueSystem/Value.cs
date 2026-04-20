using System.Collections;
using System.Reflection;
using LabApi.Features.Wrappers;
using Newtonsoft.Json.Linq;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.ValueSystem.PropertySystem;
using UnityEngine;

namespace SER.Code.ValueSystem;

public abstract class Value : IEquatable<Value>
{
    public SingleTypeOfValue Type => new(GetType());
    
    public static Type GuessValueType(Type t)
    {
        if (typeof(Value).IsAssignableFrom(t)) return t;
        if (typeof(Enum).IsAssignableFrom(t))
        {
            if (t.IsDefined(typeof(FlagsAttribute)))
            {
                return typeof(EnumFlagValue<>).MakeGenericType(t);
            }

            return typeof(EnumValue<>).MakeGenericType(t);
        };
        if (typeof(Color).IsAssignableFrom(t)) return typeof(ColorValue);
        if (t == typeof(bool)) return typeof(BoolValue);
        if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(ushort) ||
            t == typeof(int) || t == typeof(uint) || t == typeof(long) || t == typeof(ulong) ||
            t == typeof(float) || t == typeof(double) || t == typeof(decimal))
            return typeof(NumberValue);
        if (t == typeof(string)) return typeof(TextValue);
        if (t == typeof(TimeSpan)) return typeof(DurationValue);
        if (typeof(Player).IsAssignableFrom(t) || typeof(IEnumerable<Player>).IsAssignableFrom(t)) return typeof(PlayerValue);
        
        if (typeof(IEnumerable).IsAssignableFrom(t))
        {
            var itemType = t.GetInterfaces()
                .Concat([t])
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                ?.GetGenericArguments()[0] ?? typeof(object);
            
            return typeof(CollectionValue<>).MakeGenericType(GuessValueType(itemType));
        }
        
        return typeof(ReferenceValue<>).MakeGenericType(t);
    }

    public static char GetPrefixOfValue(SingleTypeOfValue value)
    {
        if (value.IsSameOrHigherThan<LiteralValue>()) return '$';
        if (value.IsSameOrHigherThan<PlayerValue>()) return '@';
        if (value.IsSameOrHigherThan<ReferenceValue>()) return '*';
        if (value.IsSameOrHigherThan<CollectionValue>()) return '&';
        return '?';
    }
    
    public abstract int HashCode { get; }

    public abstract TryGet<object> ToCSharpObject(Type targetType);

    public static Value Parse(object obj, Script? script)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
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
            string s 
                when script != null => new DynamicTextValue(s, script),
            string s                => new StaticTextValue(s),
            
            Enum e when obj.GetType().IsDefined(typeof(FlagsAttribute)) 
                => (Value)Activator.CreateInstance(typeof(EnumFlagValue<>).MakeGenericType(e.GetType()), e),
            Enum e
                => (Value)Activator.CreateInstance(typeof(EnumValue<>).MakeGenericType(e.GetType()), e),
            
            TimeSpan t              => new DurationValue(t),
            Player p                => new PlayerValue(p),
            IEnumerable<Player> ps  => new PlayerValue(ps),
            JToken t                => new ReferenceValue<JToken>(t),
            IEnumerable e           => (Value)Activator.CreateInstance(GuessValueType(obj.GetType()), e),
            Color c                 => new ColorValue(c),
            _                       => (Value)Activator.CreateInstance(typeof(ReferenceValue<>).MakeGenericType(obj.GetType()), obj),
        };
    }

    public static Dictionary<string, IValueWithProperties.PropInfo>? GetPropertiesOfValue(Type t)
    {
        if (!typeof(IValueWithProperties).IsAssignableFrom(t)) return null;
        
        if (t == typeof(TextValue))
        {
            t = typeof(StaticTextValue);
        }
        else if (typeof(ReferenceValue).IsAssignableFrom(t) && t.IsGenericType)
        {
            return ReferencePropertyRegistry.GetProperties(t.GetGenericArguments()[0]);
        }
        
        return t.CreateInstance<IValueWithProperties>().Properties;
    }
    
    public string FriendlyName => GetFriendlyName(GetType());
    
    public static string GetFriendlyName(Type t)
    {
        if (typeof(Value).IsAssignableFrom(t))
        {
            var field = t.GetField("FriendlyName", BindingFlags.Public | BindingFlags.Static);
            if (field != null)
            {
                return (string)field.GetValue(null);
            }

            if (t.BaseType != null && t.BaseType != typeof(object))
            {
                var baseName = GetFriendlyName(t.BaseType);
                if (baseName != "generic value") return baseName;
            }

            return "generic value";
        }

        if (t == typeof(object)) return "generic value";

        return t.AccurateName;
    }

    public override string ToString()
    {
        return FriendlyName;
    }

    public override int GetHashCode() => HashCode;

    public override bool Equals(object? obj)
    {
        return this == obj;
    }

    public static bool operator ==(Value? lhs, Value? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
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

    public abstract bool Equals(Value? other);
}