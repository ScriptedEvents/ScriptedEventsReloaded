namespace SER.Code.ValueSystem;

public static class ValueTypeInfo
{
    public static readonly (char, ValueType)[] PrefixToValueTable =
    [
        ('$', ValueType.Literal),
        ('@', ValueType.Player),
        ('*', ValueType.Reference),
        ('&', ValueType.Collection)
    ];
    
    public static SingleValueType SingleTypeOfValue(ValueType type)
    {
        if (SingleValueType.TypeToSingleTypeTable.TryGetValue(type, out var singleType))
        {
            return singleType;
        }

        throw new ArgumentException($"Value type {type} is not a single value type.", nameof(type));
    }
}

