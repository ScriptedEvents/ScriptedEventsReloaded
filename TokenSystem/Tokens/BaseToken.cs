using System.Linq;
using SER.Helpers.Extensions;
using SER.Helpers.ResultSystem;
using SER.ScriptSystem;
using SER.TokenSystem.Slices;
using SER.TokenSystem.Tokens.Interfaces;
using SER.TokenSystem.Tokens.VariableTokens;
using SER.ValueSystem;
using SER.VariableSystem.Variables;

namespace SER.TokenSystem.Tokens;

public class BaseToken
{
    public string RawRep { get; private set; } = null!;
    protected Slice Slice { get; private set; } = null!;
    protected Script Script { get; private set; } = null!;
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
    
    public string GetBestTextRepresentation(Script? script)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (this is TextToken textToken)
        {
            return textToken.Value;
        }
        
        if (this is IValueToken valueToken && valueToken.CanReturn<LiteralValue>(out var func) && script is not null)
        {
            if (func().WasSuccessful(out var result))
            {
                return result.ToString();
            }
        }

        return Slice.RawRep;
    }

    public override string ToString()
    {
        return GetType().AccurateName;
    }

    public TryGet<T> TryGetLiteralValue<T>() where T : LiteralValue
    {
        Result mainErr = $"Value '{RawRep}' ({GetType().AccurateName}) cannot be intrepreted as a '{typeof(T).Name}'.";
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (this is LiteralValueToken<T> literalValueToken)
        {
            return literalValueToken.Value;
        }

        if (this is IValueToken valueToken && valueToken.CanReturn<LiteralValue>(out var get))
        {
            if (get().HasErrored(out var err, out var value))
            {
                return mainErr + err;
            }

            if (value is T tValue)
            {
                return tValue;
            }

            return $"The value returned by '{RawRep}' was of type '{value.GetType().AccurateName}', " +
                   $"but a '{typeof(T).Name}' value was expected.";
        }
        
        if (this is ParenthesesToken parenthesesToken)
        {
            if (parenthesesToken.ParseExpression().HasErrored(out var err, out var result))
            {
                return mainErr + err;
            }
            
            if (Value.Parse(result) is T correctValue)
            {
                return correctValue;           
            }

            return $"Expression '{parenthesesToken.RawRep}' parsed to a {result.GetType().GetAccurateName()} " +
                   $"'{result}', which is not a '{typeof(T).Name}' value.";
        }

        if (this is VariableToken varToken)
        {
            if (varToken.TryGetVariable().HasErrored(out var error, out var variable))
            {
                return mainErr + error;
            }

            if (variable is not LiteralVariable litVariable)
            {
                return $"Variable '{varToken.RawRep}' is not a literal variable, but a {variable.GetType().AccurateName}"; 
            }

            if (litVariable.Value is not T tValue)
            {
                return $"Value of variable '{varToken.RawRep}' is not a '{typeof(T).Name}' value, " +
                       $"but a {litVariable.Value.GetType().AccurateName}.";
            }

            return tValue;
        }
        
        return mainErr;
    }

    public static TryGet<T> TryParse<T>(string rawRep, Script scr) where T : BaseToken
    {
        if (Tokenizer.TokenizeLine(rawRep, scr, null).HasErrored(out var error, out var tokens))
        {
            return error;
        }

        if (tokens.Length != 1)
        {
            return $"Value '{rawRep}' cannot represent a single {typeof(T).FriendlyTypeName()}";
        }
        
        if (tokens.First().TryCast<BaseToken, T>(rawRep).HasErrored(out error, out var tToken))
        {
            return error;
        }

        return tToken;
    }
}