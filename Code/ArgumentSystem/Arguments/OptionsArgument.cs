using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class OptionsArgument(string name, params Option[] options) : Argument(name)
{
    public readonly Option[] Options = options;

    public override string InputDescription =>
        "One of the following options:\n > " +
        string.Join("\n > ", Options.Select(o => string.IsNullOrEmpty(o.Description)
            ? o.Value
            : $"{o.Value} ({o.Description})"));

    [UsedImplicitly]
    public OldDynamicTryGet<string> GetConvertSolution(BaseToken token)
    {
        if (token.BestTextRepr().IsStatic(out var value, out var func))
        {
            return InternalConvert(value);
        }

        return new(() => InternalConvert(func()));
    }

    private OldTryGet<string> InternalConvert(string value)
    {
        var option = Options.FirstOrDefault(opt
            => opt.Value.Equals(value, StringComparison.CurrentCultureIgnoreCase));

        if (option == null)
        {
            return OldTryGet<string>.Error(
                $"Value '{value}' does not match any of the following options: " +
                $"{string.Join(", ", Options.Select(o => o.Value))}");
        }

        return OldTryGet<string>.Success(option.Value);
    }
}