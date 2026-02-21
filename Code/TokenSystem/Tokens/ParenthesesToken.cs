using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens;

public class ParenthesesToken : BaseToken, IValueToken
{
    public TypeOfValue PossibleValues => typeof(LiteralValue);
    
    public bool IsConstant => false;
    
    public BaseToken[] Tokens { get; private set; } = [];

    public string RawContent { get; private set; } = null!;

    protected override IParseResult InternalParse(Script scr)
    {
        if (Slice is not CollectionSlice { Type: CollectionBrackets.Round } slice)
        {
            return new Ignore();
        }

        RawContent = slice.Value;
        if (Tokenizer.TokenizeLine(Slice.Value, Script, LineNum)
            .HasErrored(out var tokenizeError, out var tokens))
        {
            return new Error($"In parentheses {RawRep}".AsError() + tokenizeError.AsError());
        }
        
        Tokens = tokens;
        return new Success();
    }

    public TryGet<object> ParseExpression()
    {
        if (NumericExpressionReslover.CompileExpression(Tokens).HasErrored(out var error2, out var expression))
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

        if (ValueSystem.Value.Parse(value, Script) is not LiteralValue literalValue)
        {
            return RawContent;
        }

        return literalValue;
    }
}