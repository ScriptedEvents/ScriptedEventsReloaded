using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class BoolArgument(string name) : Argument(name)
{
    public override string InputDescription => "bool (true or false) value";
    
    [UsedImplicitly]
    public DynamicTryGet<bool> GetConvertSolution(BaseToken token)
    {
        Result error = $"Value '{token.RawRep}' cannot be interpreted as a boolean value or condition.";
        if (!token.CanReturn<BoolValue>(out var get))
        {
            return error;
        }

        return new(() => get().OnSuccess(v => v.Value, error));
    }
}
