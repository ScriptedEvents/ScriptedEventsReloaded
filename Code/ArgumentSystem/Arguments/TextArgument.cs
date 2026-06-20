using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class TextArgument(string name, bool allowsSpaces = true) : Argument(name)
{
    public override string InputDescription => allowsSpaces
        ? "Any text e.g. \"Hello, World!\""
        : "Text with no spaces e.g. \"someValue\" or someValue (quotes are not required)";
    
    public bool AllowsSpaces => allowsSpaces;

    [UsedImplicitly]
    public OldDynamicTryGet<string> GetConvertSolution(BaseToken token)
    {
        if (token.BestTextRepr().IsStatic(out var value, out var func))
        {
            return value.AsOldSuccess();
        }

        return new(() => func().AsOldSuccess());
    }
}