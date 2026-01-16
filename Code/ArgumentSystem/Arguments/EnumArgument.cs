using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.Plugin.Commands.HelpSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class EnumArgument<TEnum> : Argument where TEnum : struct, Enum
{
    public EnumArgument(string name) : base(name)
    {
        HelpInfoStorage.UsedEnums.Add(typeof(TEnum));
    }
    
    public override string InputDescription => $"{typeof(TEnum).GetAccurateName()} enum value.";

    [UsedImplicitly]
    public DynamicTryGet<object> GetConvertSolution(BaseToken token)
    {
        if (InternalConvert(token).WasSuccessful(out var value))
        {
            return value;
        }

        return new(() =>
        {
            if (InternalConvert(token).HasErrored(out var error, out value))
            {
                return error;
            }

            return value;
        });
    }

    public static TryGet<TEnum> Convert(BaseToken token, Script script)
    {
        if (Convert(token, script, typeof(TEnum)).HasErrored(out var error, out var value))
        {
            return error;
        }
        
        return (TEnum)value;
    }

    public static TryGet<object> Convert(BaseToken token, Script script, Type enumType)
    {
        var stringRep = token.GetBestTextRepresentation(script);
        
        // only allow exact matches or matches with the first letter not capitalized
        if (Enum.IsDefined(enumType, stringRep) || 
            Enum.GetNames(enumType).Any(n => n.LowerFirst() == stringRep))
        {
            return Enum.Parse(enumType, stringRep, true);
        }
        
        return $"Value '{stringRep}' does not represent a valid {enumType.GetAccurateName()} " +
               $"enum value.";
    }
    
    private TryGet<TEnum> InternalConvert(BaseToken token)
    {
        return Convert(token, Script);
    }
}