using System.Collections;
using System.Runtime.InteropServices;
using LabApi.Features.Wrappers;
using Newtonsoft.Json.Linq;
using SER.Code.Exceptions;
using UnityEngine;
// ReSharper disable PropertyCanBeMadeInitOnly.Local
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace SER.Code.ValueSystem;

[StructLayout(LayoutKind.Explicit)]
public struct Value
{
    // zeros out the struct
    public Value() => this = default;
    
    [field: FieldOffset(0)] 
    public ValueType ValueType { get; private set; }

    // reference type metadata
    [FieldOffset(8)] private Type? _enumType;
    [FieldOffset(8)] private Type? _referenceType;
    
    // reference types
    [FieldOffset(16)] private string? _text;
    [FieldOffset(16)] private object? _reference;
    [FieldOffset(16)] private Player[]? _players;
    [FieldOffset(16)] private Value[]? _collection;

    // value types
    [FieldOffset(24)] private decimal _number;
    [FieldOffset(24)] private bool _bool;
    [FieldOffset(24)] private TimeSpan _duration;
    [FieldOffset(24)] private Color _color;
    
    // value type metadata
    [FieldOffset(24)] private ValueType _collectionItemValueTypes;
    
    public ValuePrefixes Prefixes => ValueTypeManager.GetPrefixesOfValue(ValueType);
    
    public static Value Text(string text) => new()
    {
        ValueType = ValueType.Text,
        _text = text
    };

    public static Value Bool(bool @bool) => new()
    {
        ValueType = ValueType.Bool,
        _bool = @bool
    };

    public static Value Number(decimal number) => new()
    {
        ValueType = ValueType.Number,
        _number = number
    };

    public static Value Duration(TimeSpan duration) => new()
    {
        ValueType = ValueType.Duration,
        _duration = duration
    };

    public static Value Player(Player player) => new()
    {
        ValueType = ValueType.Player,
        _players = [player]
    };

    public static Value Player(IEnumerable<Player> players) => new()
    {
        ValueType = ValueType.Player,
        _players = players as Player[] ?? players.ToArray(),
    };

    public static Value Enum(Enum value) => new()
    {
        ValueType = ValueType.Text,
        _text = value.ToString(),
        _enumType = value.GetType()
    };

    public static Value Reference<T>(T reference) => new()
    {
        ValueType = ValueType.Reference,
        _reference = reference,
        _referenceType = typeof(T)
    };

    public static Value Reference(object reference) => new()
    {
        ValueType = ValueType.Reference,
        _reference = reference,
        _referenceType = reference.GetType()
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
            ValueType = ValueType.Collection,
            _collection = values.ToArray(),
            _collectionItemValueTypes = itemValueTypes
        };
    }

    public static Value Color(Color color) => new()
    {
        ValueType = ValueType.Color,
        _color = color,
    };

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
}

