using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using ValueType = SER.Code.ValueSystem.ValueType;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public class TextToken : ValueToken
{
    public override ValueType ValueTypes => ValueType.Text;

    public override bool IsConstant => true;

    protected override IParseResult InternalParse(Script scr)
    {
        if (Slice is not CollectionSlice { Type: CollectionBrackets.Quotes })
        {
            return new Ignore();
        }
        
        Value = ValueSystem.Value.Text(Slice.Value);
        return new Success();
    }
}