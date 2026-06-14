using System.Runtime.InteropServices;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace SER.Code.ValueSystem;

[StructLayout(LayoutKind.Explicit)]
public struct Value()
{
    [FieldOffset(0)] private ValueType _valueType = ValueType.Unknown;
    
    [FieldOffset(sizeof(ValueType))] private string? _text = null;
    [FieldOffset(sizeof(ValueType))] private decimal _number = 0;
    [FieldOffset(sizeof(ValueType))] private bool _bool = false;
    [FieldOffset(sizeof(ValueType))] private Enum? _enum = null;
    [FieldOffset(sizeof(ValueType))] private Color _color = default;
    [FieldOffset(sizeof(ValueType))] private object? _reference = null;
    [FieldOffset(sizeof(ValueType))] private Player[] _players = [];
    [FieldOffset(sizeof(ValueType))] private Value[] _collection = [];

    public static Value FromText(string text)
    {
        return new Value
        {
            _valueType = ValueType.Text,
            _text = text
        };
    }

    public static Value Parse(object obj)
    {
        
    }
}

