using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class ReferenceArgument<TValue>(string name) : Argument(name)
{
    private static readonly string ValidInput = $"a reference to {typeof(TValue).AccurateName} object.";
    public override string InputDescription => ValidInput;

    [UsedImplicitly]
    public DynamicTryGet<TValue> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<ReferenceValue>(out var get))
        {
            return $"Value '{token.RawRep}' does not represent a valid reference.";
        }

        return new(() => get().OnSuccess(TryParse, null));
    }

    public static TryGet<TValue> TryParse(ReferenceValue value)
    {
        if (value.Value is TValue tValue)
        {
            return tValue;
        }
        
        return $"The {value} reference is not {ValidInput}";
    }
}