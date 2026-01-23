using System.Text.RegularExpressions;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens;

public class TextToken : LiteralValueToken<TextValue>
{
    protected override IParseResult InternalParse(Script scr)
    {
        if (Slice is not CollectionSlice { Type: CollectionBrackets.Quotes })
        {
            return new Ignore();
        }
        
        Value = Slice.Value;
        return new Success();
    }

    public DynamicTryGet<string> GetDynamicResolver()
    {
        if (Value.ContainsExpressions) 
            return new(() => TryGet<string>.Success(Value.ParsedValue(Script)));
        
        return DynamicTryGet.Success(Value.Value);
    }
}