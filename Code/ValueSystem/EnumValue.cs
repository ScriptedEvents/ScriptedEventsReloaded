using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Plugin.Commands.HelpSystem;

namespace SER.Code.ValueSystem;

[UsedImplicitly]
public class EnumValue<T> : TextValue where T : struct, Enum
{
    [UsedImplicitly]
    public EnumValue() : this(default) {}

    public EnumValue(T value) : base(value.ToString(), null)
    {
        if (typeof(T).IsDefined(typeof(FlagsAttribute), false))
        {
            Log.Warn($"type {typeof(T).AccurateName} uses flags = Flags enums are not supported with EnumValue");
        }
        
        EnumIndex.AddEnum(typeof(T));
    }

    [UsedImplicitly]
    public new static string FriendlyName => $"{typeof(T).Name} enum value";
    
    public static implicit operator EnumValue<T>(T value) => new(value);
}
