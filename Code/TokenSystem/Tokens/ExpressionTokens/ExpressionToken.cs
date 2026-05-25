using SER.Code.ContextSystem.Contexts;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.TokenSystem.Tokens.ExpressionTokens;

public class ExpressionToken : BaseToken, IValueToken
{
    private ValueExpressionContext? _context;
    
    protected override IParseResult InternalParse(Script scr)
    {
        if (Slice is not CollectionSlice { Type: CollectionBrackets.Curly } collection)
        {
            return new Ignore();
        }

        if (Tokenizer.TokenizeLine(collection.Value, scr, null)
            .HasErrored(out var error, out var tokensEnum))
        {
            return new Error(error);
        }
        
        var tokens = tokensEnum.ToArray();
        if (tokens.Len == 0)
        {
            return new Error($"Expression '{collection.Value}' is empty.");
        }

        _context = new ValueExpressionContext(tokens[0], false)
        {
            Script = scr
        };

        foreach (var token in tokens.Skip(1))
        {
            var res = _context.TryAddToken(token);
            if (res.ShouldContinueExecution)
            {
                continue;
            }
            
            if (res.HasErrored)
            {
                return new Error(res.ErrorMessage);
            }
        }
        
        if (_context.VerifyCurrentState().HasErrored(out var error2))
        {
            return new Error(error2);
        }

        return new Success();
    }

    public static TryGet<ExpressionToken> TryParse(CollectionSlice slice, Script script)
    {
        if (Tokenizer.GetTokenFromSlice(slice, script, null).HasErrored(out var error, out var val))
        {
            return error;
        }

        if (val is not ExpressionToken expToken)
        {
            return $"Token '{slice.RawRep}' is not an {typeof(ExpressionToken).FriendlyTypeName()}, but a {val.GetType().FriendlyTypeName()}";
        }
        
        return expToken;
    }

    public TryGet<Value> Value()
    {
        _context!.Run().MoveNext();
        return _context.GetValue();
    }
    public TypeOfValue PossibleValues => _context!.PossibleValues;
    public bool IsConstant => false;
}