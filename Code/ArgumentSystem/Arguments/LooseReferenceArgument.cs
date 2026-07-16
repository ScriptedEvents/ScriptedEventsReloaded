using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

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
    public virtual DynamicTryGet<object> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<ReferenceValue>(out var get))
        {
            return $"Value '{token.RawRep}' does not represent a valid reference.";
        }

        return new(() => get().OnSuccess(rv => TryParse(rv, type)));
    }

    public TryGet<object> TryParse(ReferenceValue value, Type targetType)
    {
        return value.ToCSharpObject(targetType);
    }
}

public class ReferenceArgument<TValue>(string name) : LooseReferenceArgument(name, typeof(TValue))
{
    public override string InputDescription => ValidInput;

    [UsedImplicitly]
    public new DynamicTryGet<TValue> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<ReferenceValue>(out var get))
        {
            return $"Value '{token.RawRep}' does not represent a valid reference.";
        }

        return new(() => get().OnSuccess(GetValue));
    }

    public static TryGet<TValue> GetValue(ReferenceValue value)
    {
        return value.GetAs<TValue>();
    }
}
