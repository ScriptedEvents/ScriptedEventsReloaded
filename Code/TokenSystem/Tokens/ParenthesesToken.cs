using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens;

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

        if (NumericExpressionReslover.CompileExpression(tokens).HasErrored(out var error2, out var expression))
        {
            return error2;
        }
        
        return expression.Evaluate();
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

    public TypeOfValue PossibleValues => new TypeOfValue<LiteralValue>();
    public bool IsConstant => false;
}