using System.Diagnostics.CodeAnalysis;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem.PropertySystem;

namespace SER.Code.ValueSystem;

public abstract class LiteralValue : Value
{
    private readonly Func<object>? _valueGetter;
    
    private static Type[]? _subclasses;
    public static Type[] Subclasses => _subclasses ??= typeof(LiteralValue).Assembly.GetTypes()
        .Where(t => 
            t.IsClass 
            && t is { IsAbstract: false, IsGenericTypeDefinition: false } 
            && typeof(LiteralValue).IsAssignableFrom(t) 
            && typeof(IValueWithProperties).IsAssignableFrom(t)
        )
        .ToArray();

    /// <summary>
    /// Initiates a new literal value.
    /// </summary>
    /// <param name="value">The underlying value OR a function returning the underlying value.</param>
    protected LiteralValue(object value)
    {
        if (value is Func<object> func)
        {
            _valueGetter = func;
            return;
        }
        
        Value = value;
    }
    
    public abstract string StringRep { get; }

    [field: AllowNull, MaybeNull]
    public object Value => field 
                           ?? _valueGetter?.Invoke() 
                           ?? throw new AndrzejFuckedUpException("literal value is null");

    public override bool Equals(Value? other) => other is LiteralValue otherP && Value.Equals(otherP.Value);

    public override string ToString()
    {
        return $"{StringRep} ({base.ToString()})";
    }

    public override int HashCode => Value.GetHashCode();

    public override TryGet<object> ToCSharpObject(Type targetType)
    {
        if (targetType.IsInstanceOfType(Value)) return Value;
        try
        {
            return Convert.ChangeType(Value, targetType);
        }
        catch
        {
            return $"Cannot convert {Value.GetType().Name} to {targetType.Name}";
        }
    }
    
    [UsedImplicitly]
    public new static string FriendlyName => "literal value";
}

public abstract class LiteralValue<T>: LiteralValue
    where T : notnull
{
    protected LiteralValue(object value) : base(value)
    {
    }

    protected LiteralValue(Func<object> valueGetter) : base(valueGetter)
    {
    }

    public new T Value => (T)base.Value;
}