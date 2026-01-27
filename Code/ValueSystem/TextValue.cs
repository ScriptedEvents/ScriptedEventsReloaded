using System.Text.RegularExpressions;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;

namespace SER.Code.ValueSystem;

public abstract class TextValue : LiteralValue<string>
{
    private static readonly Regex ExpressionRegex = new(@"~?\{.*?\}", RegexOptions.Compiled);

    /// <summary>
    /// text needs to be dynamically parsed when the value is requested, so script needs to be provided
    /// </summary>
    /// <param name="text">the text itself</param>
    /// <param name="script">script context used to parse formatting, use null when formatting is not applicable</param>
    protected TextValue(string text, Script? script) : 
        base(script is null ? text.Replace("<br>", "\n") : () => ParseValue(text.Replace("<br>", "\n"), script))
    {
    }
    
    public static implicit operator string(TextValue value)
    {
        return value.Value;
    }

    public override string StringRep => Value;
    
    public static string ParseValue(string text, Script script) => ExpressionRegex.Replace(text, match =>
    {
        if (match.Value.StartsWith("~")) return match.Value[1..];
        
        if (Tokenizer.SliceLine(match.Value).HasErrored(out var error, out var slices))
        {
            Log.Warn(script, error);
            return "<error>";
        }

        if (slices.FirstOrDefault() is not CollectionSlice { Type: CollectionBrackets.Curly } collection)
        {
            throw new AndrzejFuckedUpException();
        }
        
        // ReSharper disable once DuplicatedSequentialIfBodies
        if (ExpressionToken.TryParse(collection, script).HasErrored(out error, out var token))
        {
            Log.Warn(script, error);
            return "<error>";
        }

        if (((BaseToken)token).TryGet<LiteralValue>().HasErrored(out error, out var value))
        {
            Log.Warn(script, error);
            return "<error>";
        }
            
        return value.StringRep;
    });
}

public class DynamicTextValue(string text, Script script) : TextValue(text, script);

public class StaticTextValue(string text) : TextValue(text, null)
{
    public static implicit operator StaticTextValue(string text)
    {
        return new(text);
    }
}