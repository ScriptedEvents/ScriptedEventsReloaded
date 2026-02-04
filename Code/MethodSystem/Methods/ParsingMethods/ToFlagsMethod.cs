using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ParsingMethods;

[UsedImplicitly]
public class ToFlagsMethod : ReferenceReturningMethod<ToFlagsMethod.Flags>
{
    public record Flags(Type EnumType, Enum FlagEnum)
    {
        public TryGet<T> To<T>() where T : struct, Enum
        {
            try
            {
                return (T)FlagEnum;
            }
            catch
            {
                return $"Expected a '{typeof(T).AccurateName}' enum type, got '{EnumType}' type instead.";
            }
        }
    };

    public override string Description => "Parses wanted values into a requested enum flag.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumTypeArgument("enum flag type"),
        new TextArgument("values", false)
        {
            ConsumesRemainingValues = true
        }
    ];

    public override void Execute()
    {
        var enumType = Args.GetEnumType("enum flag type");
        var values = Args.GetRemainingArguments<string, TextArgument>("values");
        long combinedValue = 0;

        foreach (string val in values)
        {
            if (Enum.Parse(enumType, val, true) is Enum parsed)
            {
                // Convert to long to perform bitwise OR safely across different underlying types
                combinedValue |= Convert.ToInt64(parsed);
            }
        }
        
        ReturnValue = new(enumType, (Enum)Enum.ToObject(enumType, combinedValue));
    }
}