using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.Plugin.Commands.HelpSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;

namespace SER.Code.ArgumentSystem.Arguments;

public abstract class EnumArgument(string name) : Argument(name)
{
    public static TryGet<object> ConvertOne(string stringRep, Type enumType)
    {
        stringRep = stringRep.Trim();

        // only allow exact matches or matches with the first letter not capitalized
        if (Enum.IsDefined(enumType, stringRep) ||
            Enum.GetNames(enumType).Any(n => n.LowerFirst() == stringRep))
        {
            return Enum.Parse(enumType, stringRep, true);
        }

        return $"Value '{stringRep}' is not a {enumType.AccurateName} enum value.";
    }
}

public class EnumArgument<TEnum> : EnumArgument where TEnum : struct, Enum
{
    private readonly bool _isFlag;

    public EnumArgument(string name) : base(name)
    {
        EnumIndex.AddEnum(typeof(TEnum));

        if (typeof(TEnum).IsDefined(typeof(FlagsAttribute), false))
        {
            _isFlag = true;
        }
    }

    public override string InputDescription
    {
        get
        {
            var documentation = XmlDocReader.GetDocumentation(typeof(TEnum));
            return $"{typeof(TEnum).AccurateName} enum value - found using " +
                   $"'serhelp {typeof(TEnum).AccurateName}' command" +
                   (_isFlag ? ". Use '|' character to provide multiple e.g. Val1|Val2|Val3" : "") +
                   (string.IsNullOrWhiteSpace(documentation) ? "" : $". {documentation}");
        }
    }

    [UsedImplicitly]
    public DynamicTryGet<TEnum> GetConvertSolution(BaseToken token)
    {
        if (InternalConvert(token).WasSuccessful(out var value))
        {
            return value;
        }

        if (token is not IValueToken valToken || valToken.IsConstant)
        {
            return $"Not a {InputDescription}.";
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

    public static TryGet<TEnum> Convert(BaseToken token, bool isFlag)
    {
        if (!isFlag)
        {
            if (ConvertOne(token.BestStaticTextRepr(), typeof(TEnum))
                .HasErrored(out var error, out var value))
            {
                return error;
            }

            return (TEnum)value;
        }

        ulong result = 0;
        foreach (var part in token
                     .BestStaticTextRepr()
                     .Split(['|'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (ConvertOne(part, typeof(TEnum)).HasErrored(out var error, out var value))
            {
                return error;
            }

            try
            {
                result |= System.Convert.ToUInt64(value);
            }
            catch (OverflowException) // usually -1, we assume that all flags are possible then
            {
                result = ulong.MaxValue;
                break;
            }
        }
        
        return (TEnum)Enum.ToObject(typeof(TEnum), result);
    }

    public static TryGet<TEnum> Convert(string stringRep)
    {
        return ConvertOne(stringRep, typeof(TEnum)).OnSuccess(v => (TEnum)v);
    }

    private TryGet<TEnum> InternalConvert(BaseToken token)
    {
        return Convert(token, _isFlag);
    }
}
