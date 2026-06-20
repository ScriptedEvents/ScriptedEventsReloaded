using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ArgumentSystem.Arguments;

/// <summary>
///     Represents any Variable argument used in a method.
/// </summary>
public class CollectionVariableArgument(string name) : Argument(name)
{
    public override string InputDescription => "Any existing collection variable e.g. &texts or &playerIds";

    [UsedImplicitly]
    public OldDynamicTryGet<CollectionVariable> GetConvertSolution(BaseToken token)
    {
        if (token is not CollectionVariableToken variableToken)
        {
            return $"Value '{token.RawRep}' is not a collection variable.";
        }

        return new(() => variableToken.TryGetVariable());
    }
}