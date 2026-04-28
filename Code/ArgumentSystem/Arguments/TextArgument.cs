using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class TextArgument(string name, bool allowsSpaces = true) : Argument(name)
{
    public override string InputDescription => allowsSpaces
        ? "Any text e.g. \"Hello, World!\""
        : "Text with no spaces e.g. \"someValue\" or someValue (quotes are not required)";

    [UsedImplicitly]
    public DynamicTryGet<string> GetConvertSolution(BaseToken token)
    {
        if (token.BestTextRepr().IsStatic(out var value, out var func))
        {
            return value.AsSuccess();
        }

        return new(() => func().AsSuccess());
    }
}