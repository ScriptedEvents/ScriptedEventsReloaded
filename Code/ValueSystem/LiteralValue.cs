namespace SER.Code.ValueSystem;

public abstract class LiteralValue(object value) : Value
{
    public abstract string StringRep { get; }

    public object Value => value;

    public override bool EqualCondition(Value other) => other is LiteralValue otherP && Value.Equals(otherP.Value);

    public override string ToString()
    {
        return $"{StringRep} ({base.ToString()})";
    }

    public override int HashCode => Value.GetHashCode();
}

public abstract class LiteralValue<T>(T value) : LiteralValue(value)
    where T : notnull
{
    public new T Value => value;
}