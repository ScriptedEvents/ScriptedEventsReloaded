using SER.Code.ScriptSystem;
using ValueType = SER.Code.ValueSystem.ValueType;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public class BoolToken : ValueToken
{
    public override ValueType ValueTypes => ValueType.Bool;
    
    public override bool IsConstant => true;
    
    protected override IParseResult InternalParse(Script scr)
    {
        if (bool.TryParse(Slice.RawRep, out var res1))
        {
            Value = ValueSystem.Value.Bool(res1);
            return new Success();
        }
        
        return new Ignore();
    }
}