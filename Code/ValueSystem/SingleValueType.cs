namespace SER.Code.ValueSystem;

public readonly record struct SingleValueType
{
    public static readonly Dictionary<ValueType, SingleValueType> TypeToSingleTypeTable = new()
    {
        { ValueType.Text, new SingleValueType(ValueType.Text) },
        { ValueType.Number, new SingleValueType(ValueType.Number) },
        { ValueType.Bool, new SingleValueType(ValueType.Bool) },
        { ValueType.Duration, new SingleValueType(ValueType.Duration) },
        { ValueType.Color, new SingleValueType(ValueType.Color) },
        { ValueType.Player, new SingleValueType(ValueType.Player) },
        { ValueType.Reference, new SingleValueType(ValueType.Reference) },
        { ValueType.Collection, new SingleValueType(ValueType.Collection) }
    };
    
    public static SingleValueType Text = TypeToSingleTypeTable[ValueType.Text];
    public static SingleValueType Number = TypeToSingleTypeTable[ValueType.Number];
    public static SingleValueType Bool = TypeToSingleTypeTable[ValueType.Bool];
    public static SingleValueType Duration = TypeToSingleTypeTable[ValueType.Duration];
    public static SingleValueType Color = TypeToSingleTypeTable[ValueType.Color];
    public static SingleValueType Player = TypeToSingleTypeTable[ValueType.Player];
    public static SingleValueType Reference = TypeToSingleTypeTable[ValueType.Reference];
    public static SingleValueType Collection = TypeToSingleTypeTable[ValueType.Collection];
    
    public ValueType ValueType { get; }
    public char Prefix { get; }

    public SingleValueType(ValueType valueType)
    {
        if (!valueType.IsOneValue)
        {
            throw new ArgumentException("Value type must be one value.", nameof(valueType));
        }
        
        ValueType = valueType;
        foreach (var (prefix, tableType) in ValueTypeInfo.PrefixToValueTable)
        {
            if (tableType.FallsOnlyWithinExpectedValueRange(ValueType))
            {
                Prefix = prefix;
                return;
            }
        }
    }
}