namespace SER.Code.ValueSystem.Other;

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
    
    public static implicit operator TypeOfValue(Type type) => new SingleTypeOfValue(type);
}

public class TypesOfValue : TypeOfValue
{
    public TypesOfValue(params SingleTypeOfValue[] types) : base(types.Select(t => t.Type).ToArray())
    {
        _types = types.Select(t => t.Type).ToArray();
    }

    public TypesOfValue(params Type[] types) : base(types)
    {
        _types = types;
    }

    private readonly Type[] _types;
    public override string ToString() => string.Join(" or ", _types.Select(Value.GetFriendlyName));
}

public class UnknownTypeOfValue() : TypeOfValue((Type?)null)
{
    public override string ToString() => "unknown value";
}

public class SingleTypeOfValue(Type type) : TypeOfValue(type)
{
    public readonly Type Type = type;

    public bool Is(SingleTypeOfValue otherType) => otherType.Type == Type;
    public bool Is<T>() where T : Value => Is(typeof(T));
    
    public bool IsSameOrHigherThan(SingleTypeOfValue otherType) => otherType.Type.IsAssignableFrom(Type);
    public bool IsSameOrHigherThan<T>() where T : Value => IsSameOrHigherThan(typeof(T));
    
    public bool CanHold(SingleTypeOfValue otherType) => Type.IsAssignableFrom(otherType.Type);
    public bool CanHold<T>() where T : Value => CanHold(typeof(T));
    
    public override string ToString() => Value.GetFriendlyName(Type);
    
    public static implicit operator SingleTypeOfValue(Type type) => new(type);
}

public class TypeOfValue<T>() : SingleTypeOfValue(typeof(T))
    where T : Value;