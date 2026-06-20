namespace SER.Code.ValueSystem;

public readonly record struct CollectionItemValueMetadata
{
    public ValueType Type { get; init; }
    public Type? EnumType { get; init; }
    public Type? ReferenceType { get; init; }
}