using JetBrains.Annotations;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Extensions;
using SER.Helpers.ResultSystem;
using SER.TokenSystem.Tokens;

namespace SER.ArgumentSystem.Arguments;

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