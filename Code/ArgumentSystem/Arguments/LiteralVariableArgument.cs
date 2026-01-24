using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ArgumentSystem.Arguments;

public class LiteralVariableArgument(string name) : Argument(name)
{
    public override string InputDescription => "Any existing literal variable e.g. $text or $playerId";
    
    [UsedImplicitly]
    public DynamicTryGet<LiteralVariable> GetConvertSolution(BaseToken token)
    {
        if (token is not LiteralVariableToken literalVariableToken)
        {
            return $"Value '{token.RawRep}' is not a syntactically valid literal variable.";
        }

        return new(() => literalVariableToken.TryGetVariable());
    }
}