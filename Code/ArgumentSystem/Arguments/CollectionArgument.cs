using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class CollectionArgument(string name) : Argument(name)
{
    public override string InputDescription => $"A collection variable e.g. {CollectionVariableToken.Example}";

    [UsedImplicitly]
    public OldDynamicTryGet<CollectionValue> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<CollectionValue>(out var func))
        {
            return $"Value '{token.RawRep}' does not represent a collection";
        }

        return new(() => func());
    }
}