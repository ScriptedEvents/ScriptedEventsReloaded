using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class LooseReferenceArgument(string name, Type type) : Argument(name)
{
    // rider optimization :tf:
    protected string ValidInput
    {
        get => $"a reference to {field} object.";
    } = type != typeof(object) ? type.AccurateName : "any";

    public override string InputDescription => ValidInput;

    [UsedImplicitly]
    public virtual OldDynamicTryGet<object> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<ReferenceValue>(out var get))
        {
            return $"Value '{token.RawRep}' does not represent a valid reference.";
        }

        return new(() => get().OnSuccess(rv => TryParse(rv, type)));
    }

    public OldTryGet<object> TryParse(ReferenceValue value, Type targetType)
    {
        if (targetType.IsInstanceOfType(value.Value))
        {
            return value.Value;
        }

        return $"The {value} reference is not {ValidInput}";
    }
}

public class ReferenceArgument<TValue>(string name) : LooseReferenceArgument(name, typeof(TValue))
{
    public override string InputDescription => ValidInput;

    [UsedImplicitly]
    public new OldDynamicTryGet<TValue> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<ReferenceValue>(out var get))
        {
            return $"Value '{token.RawRep}' does not represent a valid reference.";
        }

        return new(() => get().OnSuccess(GetValue));
    }

    public static OldTryGet<TValue> GetValue(ReferenceValue value)
    {
        if (value.Value is TValue tValue)
        {
            return tValue;
        }

        return $"The {value} reference is not valid {typeof(TValue).AccurateName} object";
    }
}