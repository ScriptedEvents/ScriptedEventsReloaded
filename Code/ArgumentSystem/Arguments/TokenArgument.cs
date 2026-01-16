using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class TokenArgument<T>(string name) : Argument(name) where T : BaseToken
{
    // todo: add better descriptions for tokens
    public override string InputDescription => $"A {typeof(T).FriendlyTypeName(true)}";
    
    [UsedImplicitly]
    public DynamicTryGet<T> GetConvertSolution(BaseToken token)
    {
        if (token is not T cToken)
        {
            return $"Token '{token.RawRep}' is not of type {nameof(T)}.";
        }
        
        return cToken;
    }
}