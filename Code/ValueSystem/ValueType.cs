namespace SER.Code.ValueSystem;

[Flags]
public enum ValueType : ushort
{
    Unknown = 0,
    
    Text = 1 << 1,
    Number = 1 << 2,
    Bool = 1 << 3,
    Duration = 1 << 4,
    // Enum = 1 << 5,
    Color = 1 << 6,
    Literal = Text | Number | Bool | Duration | Color,
    
    Player = 1 << 7,
    
    Reference = 1 << 8,
    
    Collection = 1 << 9,
    
    Any = Literal | Player | Reference | Collection
}
