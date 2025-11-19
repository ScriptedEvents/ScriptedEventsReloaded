using SER.Helpers.Extensions;
using SER.Helpers.ResultSystem;

namespace SER.ValueSystem;

public abstract class LiteralValue(object value) : Value
{
    public abstract string StringRep { get; }
    
    public object Value => value;

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
        
        return $"Value is not of type {typeof(T).FriendlyTypeName()}, but {Value.FriendlyTypeName()}.";
    }

    public string Serialize() => Value.ToString();
}

public abstract class LiteralValue<T>(T value) : LiteralValue(value) 
    where T : notnull
{
    public new T Value => value;
}

