using System.Diagnostics.CodeAnalysis;
using SER.Code.Exceptions;

namespace SER.Code.ValueSystem;

public abstract class LiteralValue : Value
{
    private readonly Func<object>? _valueGetter;
    
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

    public override bool EqualCondition(Value other) => other is LiteralValue otherP && Value.Equals(otherP.Value);

    public override string ToString()
    {
        return $"{StringRep} ({base.ToString()})";
    }

    public override int HashCode => Value.GetHashCode();
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