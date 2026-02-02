using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using SER.Code.Helpers.Exceptions;

namespace SER.Code.Helpers.ResultSystem;

public sealed class TryGet<TValue>(TValue? value, string? errorMsg)
{
    public TValue? Value => value;
    public string? ErrorMsg => errorMsg;
    private bool WasSuccess => string.IsNullOrEmpty(errorMsg);
    public Result Result => new(WasSuccess, ErrorMsg ?? "");

    [Pure]
    public bool HasErrored()
    {
        return !WasSuccess;
    }
    
    [Pure]
    public bool HasErrored([NotNullWhen(true)] out string? error)
    {
        error = ErrorMsg;
        return !WasSuccess;
    }

    [Pure]
    public bool HasErrored([NotNullWhen(true)] out string? error, [NotNullWhen(false)] out TValue? val)
    {
        error = ErrorMsg;
        val = Value!;
        return !WasSuccess;
    }
    
    [Pure]
    public bool WasSuccessful()
    {
        return WasSuccess;
    }
    
    [Pure]
    public bool WasSuccessful([NotNullWhen(true)] out TValue? val)
    {
        val = Value!;
        return WasSuccess;
    }

    [Pure]
    public static implicit operator string(TryGet<TValue> result)
    {
        return result.ErrorMsg ?? throw new AndrzejFuckedUpException("implicit operator string(TryGet<TValue> result) called when not errored");
    }

    [Pure]
    public static implicit operator TryGet<TValue>(TValue value)
    {
        return new TryGet<TValue>(value, string.Empty);
    }

    [Pure]
    public static implicit operator TryGet<TValue>(Result res)
    {
        if (res.HasErrored(out var msg)) return new TryGet<TValue>(default, msg);

        throw new AndrzejFuckedUpException("implicit operator TryGet<TValue>(Result res) called when not errored");
    }

    [Pure]
    public static implicit operator TryGet<TValue>(string msg)
    {
        return new TryGet<TValue>(default, msg);
    }
    
    [Pure]
    public static TryGet<TValue> Error(string errorMsg)
    {
        return new TryGet<TValue>(default, errorMsg);
    }
    
    [Pure]
    public static TryGet<TValue> Success(TValue value)
    {
        return new TryGet<TValue>(value, null);
    }
    
    [Pure]
    public TryGet<TTarget> OnSuccess<TTarget>(Func<TValue, TTarget> transform, string? mainErr)
    {
        if (HasErrored(out var error, out var val))
        {
            if (mainErr is not null) 
                return mainErr + new Result(false, error);
            
            return error;
        }
        
        return transform(val);
    }
    
    [Pure]
    public TryGet<TTarget> OnSuccess<TTarget>(Func<TValue, TryGet<TTarget>> transform, string? mainErr)
    {
        if (HasErrored(out var error, out var val))
        {
            if (mainErr is not null) 
                return mainErr + new Result(false, error);
            
            return error;
        }

        return transform(val);
    }
}