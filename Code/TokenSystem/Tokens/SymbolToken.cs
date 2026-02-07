namespace SER.Code.TokenSystem.Tokens;

public class SymbolToken : BaseToken
{
    public bool IsJoker => RawRep == "*";
    
    protected override IParseResult InternalParse()
    {
        return RawRep.All(c => char.IsSymbol(c) || char.IsPunctuation(c))
            ? new Success()
            : new Ignore();
    }
}