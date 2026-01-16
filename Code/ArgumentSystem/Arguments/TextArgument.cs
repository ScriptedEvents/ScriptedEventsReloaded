using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class TextArgument(string name) : Argument(name)
{
    public override string InputDescription => "Any text e.g. \"Hello, World!\"";

    [UsedImplicitly]
    public DynamicTryGet<string> GetConvertSolution(BaseToken token)
    {
        if (token is TextToken textToken)
        {
            return textToken.GetDynamicResolver();
        }    
        
        if (token is not IValueToken valToken || !valToken.CanReturn<LiteralValue>(out var get))
        {
            return DynamicTryGet.Error($"Value '{token.RawRep}' cannot represent text.");
        }

        if (valToken.IsConstant)
        {
            return get().OnSuccess(v => v.StringRep, null);
        }

        return new(() => get().OnSuccess(v => v.StringRep, null));
    }
}