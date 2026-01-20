namespace SER.Code.ValueSystem;

public abstract class TypeOfValue
{
    protected TypeOfValue(Type[] required)
    {
        Required = required;
    }
    
    protected TypeOfValue(Type? required)
    {
        if (required is null) Required = null;
        else Required = [required];
    }
    
    public Type[]? Required { get; }
    
    public bool AreKnown(out Type[] known) => (known = Required!) is not null;

    public abstract override string ToString();
}

public class TypesOfValue(Type[] types) : TypeOfValue(types)
{
    private readonly Type[] _types = types;
    public override string ToString() => $"{string.Join(" or ", _types.Select(Value.FriendlyName))} value";
}

public class UnknownTypeOfValue() : TypeOfValue((Type?)null)
{
    public override string ToString() => "unknown value";
}

public class SingleTypeOfValue(Type type) : TypeOfValue(type)
{
    public readonly Type Type = type;
    public override string ToString() => $"{Value.FriendlyName(Type)} value";
}

public class TypeOfValue<T>() : SingleTypeOfValue(typeof(T))
    where T : Value;