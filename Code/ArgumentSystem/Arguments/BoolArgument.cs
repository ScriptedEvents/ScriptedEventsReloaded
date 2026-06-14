using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class BoolArgument(string name) : Argument(name)
{
    public override string InputDescription => "bool (true or false) value";

    [UsedImplicitly]
    public DynamicTryGet<bool> GetConvertSolution(BaseToken token)
    {
        if (token.CanReturn<BoolValue>(out var func))
        {
            return new(() => func().OnSuccess(v => v.Value));
        }

        return GenericError(token);
    }
}