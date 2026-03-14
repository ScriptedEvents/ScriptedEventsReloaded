using UnityEngine;

namespace SER.Code.ValueSystem;

public class ColorValue(Color color) : LiteralValue<Color>(color)
{
    public override string StringRep => Value.ToHex();
}