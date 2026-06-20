using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using ValueType = SER.Code.ValueSystem.ValueType;

namespace SER.Code.TokenSystem.Tokens;

public class ParenthesesToken : BaseToken, IValueToken
{
    public ValueType PossibleValueTypes => ValueType.Literal;
    
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
            return new Error($"In parentheses {RawRep}".AsOldError() + tokenizeError.AsOldError());
        }
        
        Tokens = tokens;
        return new Success();
    }

    public OldTryGet<object> ParseExpression()
    {
        if (NumericExpressionReslover.CompileExpression(Tokens).HasErrored(out var error2, out var expression))
        {
            return error2;
        }
        
        return expression.Evaluate();
    }

    public OldTryGet<Value> Value()
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
}