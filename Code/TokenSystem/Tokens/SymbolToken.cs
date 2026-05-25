using SER.Code.ScriptSystem;

namespace SER.Code.TokenSystem.Tokens;

public class SymbolToken : BaseToken
{
    public const string Joker = "*";
    public const string Floor = "_";
    public const string Arrow = "->";
    
    public bool IsJoker => RawRep == Joker;
    public bool IsFloor => RawRep == Floor;
    public bool IsArrow => RawRep == Arrow;
    
    protected override IParseResult InternalParse(Script scr)
    {
        return RawRep.All(c => char.IsSymbol(c) || char.IsPunctuation(c))
            ? new Success()
            : new Ignore();
    }
}