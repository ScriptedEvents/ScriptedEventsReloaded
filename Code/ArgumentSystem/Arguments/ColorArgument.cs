using System.Globalization;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
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
        if (TryParseColor(token.GetBestTextRepresentation(Script)).WasSuccessful(out var value))
        {
            return value;
        }

        return new(() => TryParseColor(token.GetBestTextRepresentation(Script)));
    }

    public static TryGet<Color> TryParseColor(string value)
    {
        var initValue = value;
        if (value.StartsWith("#"))
        {
            value = value.Substring(1);
        }
        
        switch (value.Length)
        {
            // RRGGBB
            case 6 when uint.TryParse(value, NumberStyles.HexNumber, null, out uint hexVal):
            {
                float r = ((hexVal & 0xFF0000) >> 16) / 255f;
                float g = ((hexVal & 0x00FF00) >> 8) / 255f;
                float b = (hexVal & 0x0000FF) / 255f;
                return new Color(r, g, b, 1f);
            }
        
            // RRGGBBAA
            case 8 when uint.TryParse(value, NumberStyles.HexNumber, null, out uint hexVal):
            {
                float r = ((hexVal & 0xFF000000) >> 24) / 255f;
                float g = ((hexVal & 0x00FF0000) >> 16) / 255f;
                float b = ((hexVal & 0x0000FF00) >> 8) / 255f;
                float a = (hexVal & 0x000000FF) / 255f;
                return new Color(r, g, b, a);
            }
            default:
                return $"Invalid color format. Expected RRGGBB (6) or RRGGBBAA (8), got '{initValue}' ({value.Length}).";
        }
    }
}