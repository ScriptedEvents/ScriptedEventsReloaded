using JetBrains.Annotations;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.ResultSystem;
using SER.TokenSystem.Tokens;
using SER.TokenSystem.Tokens.VariableTokens;
using SER.VariableSystem.Bases;
using SER.VariableSystem.Variables;

namespace SER.ArgumentSystem.Arguments;

/// <summary>
/// Represents any Variable argument used in a method.
/// </summary>
public class CollectionVariableArgument(string name) : Argument(name)
{
    public override string InputDescription => "Any existing collection variable e.g. &texts or &playerIds";

    [UsedImplicitly]
    public DynamicTryGet<CollectionVariable> GetConvertSolution(BaseToken token)
    {
        if (token is not CollectionVariableToken variableToken)
        {
            return $"Value '{token.RawRep}' is not a collection variable.";
        }

        return new(() => variableToken.TryGetVariable());
    }
}