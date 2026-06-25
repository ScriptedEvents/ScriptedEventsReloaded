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
    
    public static ValueMetadata EnumFlags(Type enumType) => new()
    {
        ValueType = ValueType.Collection,
        EnumType = enumType,
        CollectionItemMetadata = new() { Type = ValueType.Text }
    };

    public static ValueMetadata Enum(Type enumType) => new()
    {
        ValueType = ValueType.Text,
        EnumType = enumType
    };
    
    public static ValueMetadata Reference(Type referenceType) => new()
    {
        ValueType = ValueType.Reference,
        ReferenceType = referenceType
    };
    
    public static ValueMetadata Reference<T>() => new()
    {
        ValueType = ValueType.Reference,
        ReferenceType = typeof(T)
    };
    
    public static ValueMetadata Collection(CollectionItemValueMetadata collectionItemMetadata) => new()
    {
        ValueType = ValueType.Collection,
        CollectionItemMetadata = collectionItemMetadata
    };
}