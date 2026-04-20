using JetBrains.Annotations;
using SER.Code.Extensions;
using SER.Code.Plugin.Commands.HelpSystem;

namespace SER.Code.ValueSystem;

public class EnumFlagValue<T> : CollectionValue<EnumValue<T>> where T : struct, Enum
{
    [UsedImplicitly]
    public EnumFlagValue() : this(default) {}

    public EnumFlagValue(T value) : base(value.GetFlags())
    {
        HelpInfoStorage.UsedEnums.Add(typeof(T));
    }

    [UsedImplicitly]
    public new static string FriendlyName = $"collection of {typeof(T).Name} enum values";
}