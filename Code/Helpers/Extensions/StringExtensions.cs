using JetBrains.Annotations;
using SER.Code.Helpers.ResultSystem;

namespace SER.Code.Helpers.Extensions;

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
        return str.Substring(0, 1).ToLower() + str.Substring(1);
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
}