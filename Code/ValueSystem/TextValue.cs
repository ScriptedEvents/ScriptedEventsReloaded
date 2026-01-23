using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;
using System.Text.RegularExpressions;
using SER.Code.TokenSystem.Structures;

namespace SER.Code.ValueSystem;

public class TextValue(string rawValue) : LiteralValue<string>(rawValue)
{
    private readonly string _rawValue = rawValue;
    private static readonly Regex ExpressionRegex = new(@"~?\{.*?\}", RegexOptions.Compiled);

    public string ParsedValue(Script script) => ContainsExpressions ? ParseValue(_rawValue, script) : _rawValue;

    public bool ContainsExpressions => ExpressionRegex.IsMatch(_rawValue);

    public static implicit operator TextValue(string value)
    {
        return new(value);
    }
    
    public static implicit operator string(TextValue value)
    {
        return value.Value;
    }

    public override string StringRep => Value;
    
    public static string ParseValue(string text, Script script) => ExpressionRegex.Replace(text, match =>
    {
        if (match.Value.StartsWith("~")) return match.Value.Substring(1);
        
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