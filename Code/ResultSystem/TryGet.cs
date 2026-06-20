using NorthwoodLib.Pools;

namespace SER.Code.ResultSystem;

using System;
using System.Diagnostics.CodeAnalysis;

public readonly struct TryGet<T>
{
    private readonly T _value;
    private readonly bool _success;

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => _success;
    public ErrorList? Errors { get; private init; }

    private TryGet(T value, string? error, bool success)
    {
        _value = value;
        Errors = error is not null ? new ErrorList(ListPool<string>.Shared.Rent(16)) : null;
        _success = success;
    }
    
    private TryGet(T value, ErrorList? error, bool success)
    {
        _value = value;
        Errors = error;
        _success = success;
    }

    public static TryGet<T> Success(T value) => new(value, (ErrorList?)null, true);
    public static TryGet<T> Failure(string error) => new(default!, error, false);
    public static TryGet<T> Failure(ErrorList errorList) => new(default!, errorList, false);

    public T Value => _success ? _value : throw new InvalidOperationException($"tried to access value of failed TryGet<{typeof(T).Name}>");

    public bool HasErrored([NotNullWhen(true)] out ErrorList? errors, [NotNullWhen(false)] out T value)
    {
        errors = Errors;
        value = _value;
        return !_success;
    }

    public bool HasSucceeded([NotNullWhen(true)] out T value)
    {
        value = _value;
        return _success;
    }

    public static implicit operator TryGet<T>(ErrorList errorList)
    {
        return Failure(errorList);
    }

    public static implicit operator TryGet<T>(T value)
    {
        return Success(value);
    }
}