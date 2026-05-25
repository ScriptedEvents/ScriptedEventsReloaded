using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public class TextToken : LiteralValueToken<TextValue>
{
    public bool IsDynamic => Value is DynamicTextValue;
    
    protected override IParseResult InternalParse(Script scr)
    {
        if (Slice is not CollectionSlice { Type: CollectionBrackets.Quotes })
        {
            return new Ignore();
        }

        if (!TextValue.HasExpression(Slice.Value))
        {
            Value = new StaticTextValue(Slice.Value);
            return new Success();
        }

        if (TextValue.Lint(Slice.Value, scr).HasErrored(out var error))
        {
            return new Error(error);
        }

        Value = new DynamicTextValue(Slice.Value, scr);
        return new Success();
    }
}