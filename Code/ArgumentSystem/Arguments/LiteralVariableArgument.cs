using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class LiteralVariableArgument(string name) : Argument(name)
{
    public override string InputDescription => "A literal variable name (doesnt have to be real)";
    
    [UsedImplicitly]
    public DynamicTryGet<LiteralVariableToken> GetConvertSolution(BaseToken token)
    {
        if (token is not LiteralVariableToken literalVariableToken)
        {
            return $"Value '{token.RawRep}' is not a syntactically valid literal variable.";
        }

        return literalVariableToken;
    }
}