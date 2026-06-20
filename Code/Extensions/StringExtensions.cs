using System.Runtime.CompilerServices;
using System.Text;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.ResultSystem;
using SER.Code.ScriptSystem;

namespace SER.Code.Extensions;

public static class StringExtensions
{
    /// <param name="str">The input string to be modified.</param>
    extension(string str)
    {
        /// <summary>
        /// Converts the first character of the given string to lowercase while keeping the rest of the string unchanged.
        /// </summary>
        /// <returns>A new string with the first character decapitalized.</returns>
        [Pure]
        public string LowerFirst()
        {
            return str[0].ToString().ToLowerInvariant() + str[1..];
        }
        
        [Pure]
        public string Join(IEnumerable<string> values)
        {
            return string.Join(str, values);
        }
        
        [Pure]
        public OldResult AsOldError()
        {
            return new OldResult(false, str);
        }
        
        [Pure]
        public OldTryGet<string> AsOldSuccess()
        {
            return OldTryGet<string>.Success(str);
        }

        [Pure] 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ErrorList AsError() => new(str); 
        
        [Pure]
        public string Spaceify(bool lowerCase = false)
        {
            StringBuilder res = new();
            for (var index = 0; index < str.Length; index++)
            {
                var c = str[index];
                if (!char.IsUpper(c) || index == 0)
                {
                    res.Append(c);
                    continue;
                }

                res.Append(lowerCase ? $" {c.ToString().ToLowerInvariant()}" : $" {c}");
            }

            return res.ToString();
        }
        
        [Pure]
        public StaticTextValue ToStaticTextValue()
        {
            return new StaticTextValue(str);
        }
        
        [Pure]
        public DynamicTextValue ToDynamicTextValue(Script script)
        {
            return new DynamicTextValue(str, script);
        }
    }

    // python ahh

}