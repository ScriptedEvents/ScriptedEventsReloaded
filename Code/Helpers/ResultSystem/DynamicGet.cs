using System.Diagnostics.CodeAnalysis;
using SER.Code.Exceptions;

namespace SER.Code.Helpers.ResultSystem;

public class DynamicGet<T>
{
    public DynamicGet(T? value)
    {
        _value = value;
    }

    public DynamicGet(Func<T>? valueFunc)
    {
        _valueFunc = valueFunc;
    }

    private readonly T? _value;
    private readonly Func<T>? _valueFunc;
    
    public static implicit operator DynamicGet<T>(T value) => new(value: value);

    public static implicit operator DynamicGet<T>(Func<T> valueFunc) => new(valueFunc: valueFunc);

    public T Value => _value ?? _valueFunc!.Invoke();

    public bool IsStatic([NotNullWhen(true)] out T? value, [NotNullWhen(false)] out Func<T>? func)
    {
        value = _value;
        func = _valueFunc;
        if (value is not null) return true;
        if (func is not null) return false;
        throw new CoreInvariantException("DynamicGet has both null");
    }
}