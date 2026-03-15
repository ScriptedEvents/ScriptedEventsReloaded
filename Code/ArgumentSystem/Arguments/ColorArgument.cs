using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.ValueSystem;
using UnityEngine;

namespace SER.Code.ArgumentSystem.Arguments;

public class ColorArgument(string name) : Argument(name)
{
    public override string InputDescription => "Color in format RRGGBB or RRGGBBAA.";

    [UsedImplicitly]
    public DynamicTryGet<Color> GetConvertSolution(BaseToken token)
    {
        if (token is ColorToken colorToken)
        {
            return colorToken.Value.Value;
        }
        
        return new(() => token.TryGetLiteralValue<ColorValue>().OnSuccess(val => val.Value));
    }
}