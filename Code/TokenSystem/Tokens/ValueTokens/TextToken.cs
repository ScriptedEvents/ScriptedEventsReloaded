using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public class TextToken : LiteralValueToken<TextValue>
{
    protected override IParseResult InternalParse(Script scr)
    {
        if (Slice is not CollectionSlice { Type: CollectionBrackets.Quotes })
        {
            return new Ignore();
        }
        
        Value = new DynamicTextValue(Slice.Value, scr);
        return new Success();
    }
    
    public DynamicTryGet<string> GetDynamicResolver()
    {
        if (Value is DynamicTextValue) 
            return new(() => TryGet<string>.Success(Value));
        
        return DynamicTryGet.Success(Value.Value);
    }
}