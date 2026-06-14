namespace SER.Code.ValueSystem;

public struct TextValue
{
    public Value Value { get; private set; }

    public static TextValue New(string text)
    {
        return new TextValue
        {
            Value = Value.Text(text)
        };
    }
}