using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LabApi.Features.Wrappers;
using Newtonsoft.Json.Linq;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.ResultSystem;
using UnityEngine;
// ReSharper disable PropertyCanBeMadeInitOnly.Local
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace SER.Code.ValueSystem;

[StructLayout(LayoutKind.Explicit)]
public struct Value
{
    // zeros out the struct
    public Value() => this = default;

    // reference types
    [FieldOffset(0)] private string? _text;
    [FieldOffset(0)] private object? _reference;
    [FieldOffset(0)] private Player[]? _players;
    [FieldOffset(0)] private Value[]? _collection;

    // value types
    [FieldOffset(8)] private decimal _number;
    [FieldOffset(8)] private bool _bool;
    [FieldOffset(8)] private TimeSpan _duration;
    [FieldOffset(8)] private Color _color;

    [field: FieldOffset(8 + 16)]
    public ValueMetadata Metadata { get; private set; }
    
    public ValueType ValueType => Metadata.ValueType;

    public char Prefix => Metadata.ValueType.Prefixes.Single;
    
    public static Value Text(string text) => new()
    {
        Metadata = new() { ValueType = ValueType.Text },
        _text = text
    };
    
    public static Value Bool(bool @bool) => new()
    {
        Metadata = new() { ValueType = ValueType.Bool },
        _bool = @bool
    };

    public static Value Number(decimal number) => new()
    {
        Metadata = new() { ValueType = ValueType.Number },
        _number = number
    };

    public static Value Duration(TimeSpan duration) => new()
    {
        Metadata = new() { ValueType = ValueType.Duration },
        _duration = duration
    };

    public static Value Player(Player player) => new()
    {
        Metadata = ValueMetadata.Basic(ValueType.Player),
        _players = [player]
    };

    public static Value Player(IEnumerable<Player> players) => new()
    {
        Metadata = new() { ValueType = ValueType.Player },
        _players = players as Player[] ?? players.ToArray(),
    };

    public static Value Enum(Enum value) => new()
    {
        Metadata = ValueMetadata.Enum(value.GetType()),
        _text = value.ToString(),
    };

    public static Value Reference<T>(T reference) => new()
    {
        Metadata = ValueMetadata.Reference<T>(),
        _reference = reference,
    };

    public static Value Reference(object reference) => new()
    {
        Metadata = new()
        {
            ValueType = ValueType.Reference,
            ReferenceType = reference.GetType()
        },
        _reference = reference,
    };
    
    public static Value Collection(IEnumerable collection)
    {
        List<Value> values = [];
        ValueType itemValueTypes = ValueType.Invalid;
        foreach (var item in collection)
        {
            var value = Parse(item);
            values.Add(value);
            itemValueTypes |= value.ValueType;
        }

        return new Value
        {
            Metadata = new()
            {
                ValueType = ValueType.Collection, 
                CollectionItemMetadata = new()
                {
                    Type = itemValueTypes
                }
            },
            _collection = values.ToArray(),
        };
    }

    public static Value Color(Color color) => new()
    {
        Metadata = new() { ValueType = ValueType.Color },
        _color = color,
    };

    public TryGet<string> AsText()
    {
        if (ValueType != ValueType.Text)
            return InvalidRangeError(ValueType.Text);
        
        return _text ?? throw new InvalidOperationException();
    }

    public TryGet<bool> AsBool()
    {
        if (ValueType != ValueType.Bool) 
            return InvalidRangeError(ValueType.Bool);
        
        return _bool;
    }

    public TryGet<decimal> AsNumber()
    {
        if (ValueType != ValueType.Number)
            return InvalidRangeError(ValueType.Number);

        return _number;
    }

    public TryGet<TimeSpan> AsDuration()
    {
        if (ValueType != ValueType.Duration)
            return InvalidRangeError(ValueType.Duration);

        return _duration;
    }

    public TryGet<Color> AsColor()
    {
        if (ValueType != ValueType.Color)
            return InvalidRangeError(ValueType.Color);

        return _color;
    }

    public TryGet<Player> AsPlayer()
    {
        if (ValueType != ValueType.Player)
            return InvalidRangeError(ValueType.Player);

        if (_players == null || _players.Length == 0)
            return "Value contains no players".AsError();

        return _players[0];
    }

    public TryGet<Player[]> AsPlayers()
    {
        if (ValueType != ValueType.Player)
            return InvalidRangeError(ValueType.Player);

        return _players ?? throw new InvalidOperationException();
    }
    
    public TryGet<object> AsReference()
    {
        if (ValueType != ValueType.Reference)
            return InvalidRangeError(ValueType.Reference);

        return _reference ?? throw new InvalidOperationException();
    }

    public TryGet<T> AsReference<T>()
    {
        if (ValueType != ValueType.Reference)
            return InvalidRangeError(ValueType.Reference);

        if (_reference is T t)
            return t;

        return $"Reference is not of type {typeof(T).AccurateName}".AsError();
    }

    public TryGet<Value[]> AsCollection()
    {
        if (ValueType != ValueType.Collection)
            return InvalidRangeError(ValueType.Collection);

        return _collection ?? throw new InvalidOperationException();
    }

    public static Value Parse(object obj)
    {
        if (obj is null) throw new AndrzejFuckedUpException();
        if (obj is Value v) return v;

        return obj switch
        {   
            bool b                => Bool(b),
            byte n                => Number(n),
            sbyte n               => Number(n),
            short n               => Number(n),
            ushort n              => Number(n),
            int n                 => Number(n),
            uint n                => Number(n),
            long n                => Number(n),
            ulong n               => Number(n),
            float n               => Number((decimal)n),
            double n              => Number((decimal)n),
            decimal n             => Number(n),
            string s              => Text(s),
            Enum e                => Enum(e),
            TimeSpan t            => Duration(t),
            Player p              => Player(p),
            IEnumerable<Player> p => Player(p),
            JToken t              => Reference(t),
            IEnumerable e         => Collection(e),
            Color c               => Color(c),
            _                     => Reference(obj),
        };
    }

    public static ValueType GuessValueType(Type t)
    {
        if (t == typeof(string)) return ValueType.Text;
        
        if (t == typeof(bool)) return ValueType.Bool;
        
        if (typeof(Color).IsAssignableFrom(t)) return ValueType.Color;
        
        if (typeof(Player).IsAssignableFrom(t) || typeof(IEnumerable<Player>).IsAssignableFrom(t)) 
            return ValueType.Player;
        
        if (t == typeof(TimeSpan)) return ValueType.Duration;
        
        if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(ushort) ||
            t == typeof(int) || t == typeof(uint) || t == typeof(long) || t == typeof(ulong) ||
            t == typeof(float) || t == typeof(double) || t == typeof(decimal))
            return ValueType.Number;
        
        if (typeof(IEnumerable).IsAssignableFrom(t))
        {
            var itemType = GetEnumerableItemType(t);

            return t == itemType ? ValueType.Reference : ValueType.Collection;
        }
        
        return ValueType.Reference;
    }
    
    public static ValueMetadata GuessValueMetadata(Type t)
    {
        if (t == typeof(Value)) throw new ArgumentException("Cannot guess metadata for Value type.", nameof(t));
        if (t == typeof(ValueMetadata)) throw new ArgumentException("Cannot guess metadata for ValueMetadata type.", nameof(t));
        if (t == typeof(CollectionItemValueMetadata)) throw new ArgumentException("Cannot guess metadata for CollectionItemValueMetadata type.", nameof(t));
        
        var count = 0;
        while (t.IsByRef)
        {
            if (count++ > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
                
            if (t.GetElementType() is { } element)
            {
                t = element;
            }
        }
        
        if (typeof(Enum).IsAssignableFrom(t))
        {
            if (t.IsDefined(typeof(FlagsAttribute), true))
            {
                return ValueMetadata.EnumFlags(t);
            }

            return ValueMetadata.Enum(t);
        }

        var valType = GuessValueType(t);
        if (valType is 
            ValueType.Text or 
            ValueType.Bool or 
            ValueType.Color or 
            ValueType.Duration or 
            ValueType.Number or 
            ValueType.Player)
        {
            return ValueMetadata.Basic(valType);
        }

        if (valType is ValueType.Collection)
        {
            var itemType = GetEnumerableItemType(t);

            if (itemType == t)
            {
                return ValueMetadata.Reference(t);
            }

            return ValueMetadata.Collection(new() { Type = GuessValueType(itemType) });
        }

        return ValueMetadata.Reference(t);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Type GetEnumerableItemType(Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return t.GetGenericArguments()[0];

        foreach (var i in t.GetInterfaces())
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return i.GetGenericArguments()[0];
        }

        return typeof(object);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ErrorList InvalidRangeError(ValueType range)
    {
        return new($"Value of type {ValueType} cannot be interpreted as {range.ToString()}");
    }
}

