using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using UnityEngine;

namespace SER.Code.ArgumentSystem.Arguments;

public class ColorArgument(string name) : Argument(name)
{
    public override string InputDescription => "Color in format RRGGBB or RRGGBBAA.";

    [UsedImplicitly]
    public DynamicTryGet<Color> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<ColorValue>(out var func))
        {
            return $"Value is not a {InputDescription}.";
        }

        return new(() => func().OnSuccess(val => val.Value));
    }
}