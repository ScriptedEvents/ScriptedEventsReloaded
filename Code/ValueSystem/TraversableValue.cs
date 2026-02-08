namespace SER.Code.ValueSystem;

public abstract class TraversableValue : Value
{
    public abstract Value[] TraversableValues { get; }
}