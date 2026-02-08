using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens;

public class TextToken : LiteralValueToken<TextValue>
{
    protected override IParseResult InternalParse()
    {
        if (Slice is not CollectionSlice { Type: CollectionBrackets.Quotes })
        {
            return new Ignore();
        }
        
        Value = new DynamicTextValue(Slice.Value, Script!);
        return new Success();
    }
    
    public DynamicTryGet<string> GetDynamicResolver()
    {
        if (Value is DynamicTextValue) 
            return new(() => TryGet<string>.Success(Value));
        
        return DynamicTryGet.Success(Value.Value);
    }
    
    public static TextToken GetToken(string content)
    {
        return GetToken<TextToken>($"\"{content}\"");
    }
    
    public static TextToken GetToken(params object[] parts)
    {
        if (parts.Any(p => p.GetType() != typeof(string) && !p.GetType().IsAssignableFrom(typeof(BaseToken))))
        {
            throw new InvalidDocsSymbolException("Expected only strings and expressions");
        }
        
        return GetToken<TextToken>(
            $"\"{parts.Select(p => p is BaseToken t ? t.RawRep : p.ToString()).JoinStrings(" ")}\""
        );
    }
}