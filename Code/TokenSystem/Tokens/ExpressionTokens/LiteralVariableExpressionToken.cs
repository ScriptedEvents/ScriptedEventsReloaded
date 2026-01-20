using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens.ExpressionTokens;

public class LiteralVariableExpressionToken : ExpressionToken
{
    private Safe<LiteralVariableToken> _varToken;
    
    protected override IParseResult InternalParse(BaseToken[] tokens)
    {
        if (tokens.Length != 1 || tokens.First() is not LiteralVariableToken literalVariableToken)
        {
            return new Ignore();
        }
        
        _varToken = literalVariableToken;
        return new Success();
    }

    public override TryGet<Value> Value() => _varToken.Value.Value();

    public override TypeOfValue PossibleValues => new TypeOfValue<LiteralValue>();
}