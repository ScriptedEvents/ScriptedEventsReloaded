namespace SER.Code.ValueSystem;

[Flags]
public enum ValueType : ushort
{
    Invalid    = 0,
    Text       = 1 << 0,
    Number     = 1 << 1,
    Bool       = 1 << 2,
    Duration   = 1 << 3,
    Color      = 1 << 4,
    Literal    = Text | Number | Bool | Duration | Color,
    Player     = 1 << 5,
    Reference  = 1 << 6,
    Collection = 1 << 7,
    Any        = Literal | Player | Reference | Collection
}