using System.Text.RegularExpressions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;
using SER.Code.ValueSystem.PropertySystem;
using ValueType = SER.Code.ValueSystem.Other.ValueType;

namespace SER.Code.ValueSystem;

public abstract class TextValue : LiteralValue<string>, IValueWithProperties
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
    
    [UsedImplicitly]
    public new static string FriendlyName => "text value";

    public static TryGet<ExpressionToken?> ParseExpression(string text, Script script)
    {
        if (text.StartsWith("~")) return null as ExpressionToken;
        
        if (Tokenizer.SliceLine(text).HasErrored(out var error, out var enumSlices))
        {
            return error;
        }

        if (enumSlices.ToArray() is not [CollectionSlice { Type: CollectionBrackets.Curly } collection])
        {
            return "Parsing failed";
        }
        
        // ReSharper disable once DuplicatedSequentialIfBodies
        if (ExpressionToken.TryParse(collection, script).HasErrored(out error, out var token))
        {
            return error;
        }

        return token;
    }
    
    public static bool HasExpression(string text) => ExpressionRegex.IsMatch(text);
    
    public static string ParseValue(string text, Script script) => ExpressionRegex.Replace(text, match =>
    {
        if (ParseExpression(match.Value, script).HasErrored(out var error, out var token) 
            || token is null
            || ((BaseToken)token).TryGet<LiteralValue>().HasErrored(out error, out var value))
        {
            if (string.IsNullOrEmpty(error)) return match.Value[1..];
            script.Warn(error!);
            return "<error>";
        }

        return value.StringRep;
    });
    
    public static Result Lint(string text, Script script)
    {
        foreach (var match in ExpressionRegex.Matches(text).Cast<Match>())
        {
            if (ParseExpression(match.Value, script).HasErrored(out var error))
            {
                return error;
            }
        }

        return true;
    }

    private class Prop<T>(Func<TextValue, T> handler, string? description)
        : IValueWithProperties.PropInfo<TextValue, T>(handler, description) where T : Value;

    public Dictionary<string, IValueWithProperties.PropInfo> Properties { get; } = new()
    {
        ["length"] = new Prop<NumberValue>(t => t.Value.Length, "Amount of characters in the text"),
        ["upper"] = new Prop<StaticTextValue>(t => t.Value.ToUpper(), "Upper case of the text"),
        ["lower"] = new Prop<StaticTextValue>(t => t.Value.ToLowerInvariant(), "Lower case of the text"),
        ["trim"] = new Prop<StaticTextValue>(t => t.Value.Trim(), "Trimmed text"),
        ["isEmpty"] = new Prop<BoolValue>(t => string.IsNullOrEmpty(t.Value), "Whether the text is empty"),
        ["valType"] = new Prop<EnumValue<ValueType>>(_ => ValueType.Text, "The type of the value")
    };

    public override TryGet<object> ToCSharpObject(Type targetType)
    {
        if (targetType.IsInstanceOfType(Value)) return Value;
        if (targetType.IsEnum)
        {
            try { return Enum.Parse(targetType, Value, true); }
            catch { return $"Cannot parse '{Value}' as {targetType.Name}"; }
        }
        return base.ToCSharpObject(targetType);
    }
}

[UsedImplicitly]
public class DynamicTextValue(string text, Script script) : TextValue(text, script)
{
    [UsedImplicitly]
    public DynamicTextValue() : this(string.Empty, null!) {}

    [UsedImplicitly]
    public new static string FriendlyName => "text value";
}

[UsedImplicitly]
public class StaticTextValue(string text) : TextValue(text, null)
{
    [UsedImplicitly]
    public StaticTextValue() : this(string.Empty) {}

    public static implicit operator StaticTextValue(string text)
    {
        return new(text);
    }

    [UsedImplicitly]
    public new static string FriendlyName => "text value";
}