using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class CollectionArgument(string name) : Argument(name)
{
    public override string InputDescription => $"A collection variable e.g. {CollectionVariableToken.Example}";
    
    [UsedImplicitly]
    public DynamicTryGet<CollectionValue> GetConvertSolution(BaseToken token)
    {
        if (token is not IValueToken valToken || !valToken.CanReturn<CollectionValue>(out var func))
        {
            return $"Value '{token.RawRep}' does not represent a collection";
        }

        return new(() => func());
    }
}