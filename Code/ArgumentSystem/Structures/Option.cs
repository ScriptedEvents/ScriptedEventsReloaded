using SER.Code.Extensions;
using SER.Code.Plugin.Commands.HelpSystem;

namespace SER.Code.ArgumentSystem.Structures;

public record Option(string Value, string Description = "")
{
    public static implicit operator Option(string value)
    {
        return new Option(value);
    }

    public static Option Enum<T>(string? name = null) where T : struct, Enum
    {
        HelpInfoStorage.UsedEnums.Add(typeof(T));
        return new(name ?? typeof(T).Name.LowerFirst(), $"Returns a {typeof(T).GetAccurateName()} enum value");
    }
    
    public static Option Reference<T>(string value) where T : class
    {
        return new(value, $"Returns a reference to {typeof(T).GetAccurateName()} object");
    }
}