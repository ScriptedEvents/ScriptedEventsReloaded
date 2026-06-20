using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.TokenSystem.Tokens.ValueTokens;

namespace SER.Code.TokenSystem.Tokens;

public class BaseToken
{
    public string RawRep { get; private set; } = null!;
    protected Slice Slice { get; private set; } = null!;
    public Script Script { get; private set; } = null!;
    protected uint? LineNum { get; private set; } = null;
    
    public IParseResult TryInit(Slice slice, Script script, uint? lineNum)
    {
        RawRep = slice.RawRep;
        Slice = slice;
        Script = script;
        LineNum = lineNum;
        return InternalParse(script);
    }

    public interface IParseResult;
    
    /// <summary>
    /// Used when the token is successfully parsed.
    /// </summary>
    public record struct Success : IParseResult;
    
    /// <summary>
    /// Used when the input was never intended to be this token. This will allow a different token to be parsed.
    /// </summary>
    public record struct Ignore : IParseResult;
    
    /// <summary>
    /// Used when there was a clear intent of using this token, but parsing still failed. This will cause a compile error.
    /// </summary>
    public record struct Error(string Message) : IParseResult;

    protected virtual IParseResult InternalParse(Script scr)
    {
        return new Success();
    }
    
    public string BestStaticTextRepr() => InternalBestTextExpr().Value;
    
    public OldDynamicGet<string> BestTextRepr() => InternalBestTextExpr();
    
    public OldDynamicGet<string> InternalBestTextExpr()
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (this is TextToken textToken)
        {
            if (!textToken.IsConstant)
            {
                return new(() => textToken.Value);
            }
            
            return textToken.Value.Value;
        }

        if (this is not IValueToken valueToken || !valueToken.CapableOf<LiteralValue>(out var func))
        {
            return RawRep;
        }

        if (valueToken.IsConstant)
        {
            return func().WasSuccessful(out var value) ? value.StringRep : RawRep;
        }
            
        return new(() => func().WasSuccessful(out var value) ? value.StringRep : RawRep);
    }

    public override string ToString()
    {
        return $"token '{RawRep}' ({GetType().AccurateName})";
    }

    public static OldTryGet<T> TryParse<T>(string rawRep, Script scr) where T : BaseToken
    {
        if (Tokenizer.TokenizeLine(rawRep, scr, null).HasErrored(out var error, out var tokens))
        {
            return error;
        }

        if (tokens.Length != 1)
        {
            return $"Value '{rawRep}' cannot represent a single {typeof(T).FriendlyTypeName()}";
        }
        
        if (tokens.First().TryCast<T>(rawRep).HasErrored(out error, out var tToken))
        {
            return error;
        }

        return tToken;
    }
}