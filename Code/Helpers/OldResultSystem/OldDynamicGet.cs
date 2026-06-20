using System.Diagnostics.CodeAnalysis;
using SER.Code.Exceptions;

namespace SER.Code.Helpers.OldResultSystem;

public class OldDynamicGet<T>
{
    public OldDynamicGet(T? value)
    {
        _value = value;
    }

    public OldDynamicGet(Func<T>? valueFunc)
    {
        _valueFunc = valueFunc;
    }

    private readonly T? _value;
    private readonly Func<T>? _valueFunc;
    
    public static implicit operator OldDynamicGet<T>(T value) => new(value: value);

    public static implicit operator OldDynamicGet<T>(Func<T> valueFunc) => new(valueFunc: valueFunc);

    public T Value => _value ?? _valueFunc!.Invoke();

    public bool IsStatic([NotNullWhen(true)] out T? value, [NotNullWhen(false)] out Func<T>? func)
    {
        value = _value;
        func = _valueFunc;
        if (value is not null) return true;
        if (func is not null) return false;
        throw new AndrzejFuckedUpException("DynamicGet has both null");
    }
}