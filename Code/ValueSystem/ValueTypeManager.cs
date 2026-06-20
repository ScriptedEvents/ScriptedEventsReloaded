using System.Runtime.CompilerServices;

namespace SER.Code.ValueSystem;

public static class ValueTypeManager
{
    public static ValueType[] BaseValueTypes =
    [
        ValueType.Literal,
        ValueType.Player,
        ValueType.Reference,
        ValueType.Collection
    ];
    
    public static readonly (char, ValueType)[] PrefixToValueTable =
    [
        ('$', ValueType.Literal),
        ('@', ValueType.Player),
        ('*', ValueType.Reference),
        ('&', ValueType.Collection)
    ];

    [Pure]
    public static ValuePrefixes GetPrefixesOfValue(ValueType valueType)
    {
        ValuePrefixes prefixes = new();
        
        foreach (var (prefix, type) in PrefixToValueTable)
        {
            if (ValueFallsWithinExpectedValueRange(type, valueType))
            {
                prefixes.Add(prefix);
            }
        }

        return prefixes;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ValueFallsWithinExpectedValueRange(ValueType value, ValueType range)
    {
        return (value & ~range) == 0;
    }
}