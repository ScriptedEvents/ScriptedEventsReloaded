using SER.Helpers;
using SER.Helpers.Exceptions;
using SER.Helpers.ResultSystem;
using SER.ScriptSystem;
using SER.TokenSystem.Slices;
using SER.TokenSystem.Structures;
using SER.TokenSystem.Tokens.Interfaces;
using SER.ValueSystem;

namespace SER.TokenSystem.Tokens;

public class ParenthesesToken : BaseToken, IValueToken
{
    private BaseToken[]? _tokens = null;

    public string RawContent { get; private set; } = null!;
    
    public TryGet<BaseToken[]> TryGetTokens()
    {
        if (_tokens is not null)
        {
            return _tokens;
        }

        if (Slice is null)
        {
            throw new AndrzejFuckedUpException();
        }

        Result error = $"Failed to get underlying tokens in the '{Slice.RawRep}' parentheses.";
        if (Tokenizer.TokenizeLine(Slice.Value, Script, LineNum)
            .HasErrored(out var tokenizeError, out var tokens))
        {
            return error + tokenizeError;
        }

        return _tokens = tokens.ToArray();
    }

    protected override IParseResult InternalParse(Script scr)
    {
        if (Slice is CollectionSlice { Type: CollectionBrackets.Round } slice)
        {
            RawContent = slice.Value;
            return new Success();
        }
        
        return new Ignore();
    }

    public TryGet<object> ParseExpression()
    {
        if (TryGetTokens().HasErrored(out var error, out var tokens))
        {
            return error;
        }
        
        return NumericExpressionReslover.ParseExpression(tokens);
    }

    public TryGet<Value> Value()
    {
        if (ParseExpression().HasErrored(out var error, out var value))
        {
            return error;
        }

        if (ValueSystem.Value.Parse(value) is not LiteralValue literalValue)
        {
            return RawContent;
        }

        return literalValue;
    }

    public Type[] PossibleValueTypes => [typeof(LiteralValue)];
    public bool IsConstant => false;
}