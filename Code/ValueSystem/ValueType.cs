using System.Runtime.CompilerServices;

namespace SER.Code.ValueSystem;

[Flags]
public enum ValueType : ushort
{
    Invalid    = 0,
    Text       = 1 << 0,
    Number     = 1 << 1,
    Bool       = 1 << 2,
    Duration   = 1 << 3,
    Color      = 1 << 4,
    Literal    = Text | Number | Bool | Duration | Color,
    Player     = 1 << 5,
    Reference  = 1 << 6,
    Collection = 1 << 7,
    Any        = Literal | Player | Reference | Collection,
}

public static class ValueTypeExtensions
{
    extension(ValueType value)
    {
        public SingleValueType AsSingle(bool verify)
        {
            if (verify && !value.IsOneValue)
            {
                throw new ArgumentException("Value type must be one value.", nameof(value));
            }

            return ValueTypeInfo.SingleTypeOfValue(value);
        }
        
        public bool IsOneValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ushort num = (ushort)value;
                return num != 0 && (num & num - 1) == 0;
            }
        }
        
        public ValuePrefixes Prefixes
        {
            [Pure]
            get
            {
                ValuePrefixes prefixes = new();

                foreach (var (prefix, tableType) in ValueTypeInfo.PrefixToValueTable)
                {
                    if (tableType.FallsOnlyWithinExpectedValueRange(value))
                    {
                        prefixes.Add(prefix);
                    }
                }

                return prefixes;
            }
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FallsOnlyWithinExpectedValueRange(ValueType range)
        {
            return (value & ~range) == 0;
        }
        
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string StringRepr()
        {
            // todo: improve
            return value.ToString();
        }
    }
}