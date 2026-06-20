namespace SER.Code.ValueSystem;

public readonly record struct ValueMetadata
{
    public ValueType ValueType { get; init; }
    public Type? EnumType { get; init; }
    public Type? ReferenceType { get; init; }
    public CollectionItemValueMetadata CollectionItemMetadata { get; init; }

    public static ValueMetadata Basic(ValueType valueType) => new()
    {
        ValueType = valueType
    };
}