using JetBrains.Annotations;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;

namespace SER.Code.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Converts the first character of the given string to lowercase while keeping the rest of the string unchanged.
    /// </summary>
    /// <param name="str">The input string to be modified.</param>
    /// <returns>A new string with the first character decapitalized.</returns>
    [Pure]
    public static string LowerFirst(this string str)
    {
        return str[0].ToString().ToLower() + str[1..];
    }

    // python ahh
    [Pure]
    public static string Join(this string separator, IEnumerable<string> values)
    {
        return string.Join(separator, values);
    }

    [Pure]
    public static Result AsError(this string error)
    {
        return new Result(false, error);
    }

    [Pure]
    public static string Spaceify(this string str, bool lowerCase = false)
    {
        string res = "";
        for (var index = 0; index < str.Length; index++)
        {
            var c = str[index];
            if (!char.IsUpper(c) || index == 0)
            {
                res += c;
                continue;
            }
            
            if (lowerCase)
            {
                res += $" {c.ToString().ToLower()}";
            }
            else
            {
                res += $" {c}";   
            }
        }

        return res;
    }
    
    [Pure]
    public static StaticTextValue ToStaticTextValue(this string text)
    {
        return new StaticTextValue(text);
    }

    [Pure]
    public static DynamicTextValue ToDynamicTextValue(this string text, Script script)
    {
        return new DynamicTextValue(text, script);
    }
}