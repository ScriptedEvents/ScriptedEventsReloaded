using SER.Helpers;
using SER.Helpers.ResultSystem;
using SER.TokenSystem.Tokens.VariableTokens;
using SER.ValueSystem;

namespace SER.TokenSystem.Tokens.ExpressionTokens;

public class LiteralVariableExpressionToken : ExpressionToken
{
    private MustSet<LiteralVariableToken> _varToken = null!;
    
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

    public override Type[] PossibleValueTypes => [typeof(LiteralValue)];
}