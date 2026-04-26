using JetBrains.Annotations;
using SER.Code.Exceptions;
using SER.Code.Extensions;
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
            throw new AndrzejFuckedUpException(
                $"{typeof(T).AccurateName} uses flags = Flags enums are not supported with EnumValue");
        }
        
        HelpInfoStorage.UsedEnums.Add(typeof(T));
    }

    [UsedImplicitly]
    public new static string FriendlyName => $"{typeof(T).Name} enum value";
    
    public static implicit operator EnumValue<T>(T value) => new(value);
}
