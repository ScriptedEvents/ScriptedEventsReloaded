using SER.Helpers.Extensions;
using SER.Helpers.ResultSystem;

namespace SER.ValueSystem;

public abstract class LiteralValue(object value) : Value
{
    public abstract string StringRep { get; }
    
    public object BaseValue => value;

    public override string ToString()
    {
        return StringRep;
    }

    public TryGet<T> TryGetValue<T>() where T : Value
    {
        if (this is T tValue)
        {
            return tValue;
        }
        
        return $"Value is not of type {typeof(T).FriendlyTypeName()}, but {BaseValue.FriendlyTypeName()}.";
    }

    public override bool Equals(object? obj)
    {
        var objIsCorrectValue = BaseValue.Equals(obj);
        var objIsCorrectLiteral = obj is LiteralValue other && Equals(other);
        return objIsCorrectValue || objIsCorrectLiteral;
    }

    protected bool Equals(LiteralValue other)
    {
        return BaseValue == other.BaseValue;
    }

    public override int GetHashCode()
    {
        return BaseValue.GetHashCode();
    }
}

public abstract class LiteralValue<T>(T value) : LiteralValue(value) 
    where T : notnull
{
    public T ExactValue => value;
}

