using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class BoolArgument(string name) : Argument(name)
{
    public override string InputDescription => "boolean (true or false) value";

    public bool IsFunction { get; init; } = false;
    
    [UsedImplicitly]
    public DynamicTryGet<bool> GetConvertSolution(BaseToken token)
    {
        Result error = $"Value '{token.RawRep}' cannot be interpreted as a boolean value or condition.";
        if (token is not IValueToken valueToken || !valueToken.CapableOf<BoolValue>(out var get))
        {
            return error;
        }

        return valueToken.IsConstant
            ? new(get().OnSuccess(v => v.Value, error))
            : new(() => get().OnSuccess(v => v.Value, error));
    }
}
