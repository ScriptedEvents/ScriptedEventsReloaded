using System.Globalization;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;
using UnityEngine;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public class ColorToken : LiteralValueToken<ColorValue>
{
    protected override IParseResult InternalParse(Script scr)
    {
        if (TryParseColor(RawRep).WasSuccessful(out var color))
        {
            Value = new ColorValue(color);
            return new Success();
        }
        
        return new Ignore();
    }
    
    public static TryGet<Color> TryParseColor(string value)
    {
        var initValue = value;
        if (value.StartsWith("#"))
        {
            value = value[1..];
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