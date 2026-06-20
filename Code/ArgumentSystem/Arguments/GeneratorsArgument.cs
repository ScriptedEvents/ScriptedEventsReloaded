using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class GeneratorsArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => 
        $"reference to {nameof(Generator)} " +
        $"or 'all' for every generator";

    [UsedImplicitly]
    public OldDynamicTryGet<Generator[]> GetConvertSolution(BaseToken token)
    {
        if (token is SymbolToken { IsJoker: true } or AllToken)
        {
            return new(() => Generator.List.ToArray());
        }

        if (!token.CanReturn<ReferenceValue<Generator>>(out var func))
        {
            return "Value is not a valid reference to a generator.";
        }

        return new(() => func().OnSuccess(x => new[] { x.Value }));
    }
}