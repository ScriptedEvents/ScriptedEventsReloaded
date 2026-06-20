using System.Diagnostics.CodeAnalysis;
using SER.Code.Exceptions;

namespace SER.Code.Helpers.OldResultSystem;

public sealed class OldTryGet<TValue>(TValue? value, string? errorMsg)
{
    public TValue? Value => value;
    public string? ErrorMsg => errorMsg;
    private bool WasSuccess => string.IsNullOrEmpty(errorMsg);
    public OldResult Result => new(WasSuccess, ErrorMsg ?? "");

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
    public static implicit operator OldTryGet<TValue>(TValue value)
    {
        return new OldTryGet<TValue>(value, string.Empty);
    }

    [Pure]
    public static implicit operator OldTryGet<TValue>(OldResult res)
    {
        if (res.HasErrored(out var msg)) return new OldTryGet<TValue>(default, msg);

        throw new AndrzejFuckedUpException("implicit operator TryGet<TValue>(Result res) called when not errored");
    }

    [Pure]
    public static implicit operator OldTryGet<TValue>(string msg)
    {
        return new OldTryGet<TValue>(default, msg);
    }
    
    [Pure]
    public static OldTryGet<TValue> Error(string errorMsg)
    {
        return new OldTryGet<TValue>(default, errorMsg);
    }
    
    [Pure]
    public static OldTryGet<TValue> Success(TValue value)
    {
        return new OldTryGet<TValue>(value, null);
    }
    
    [Pure]
    public OldTryGet<TTarget> OnSuccess<TTarget>(Func<TValue, TTarget> transform, string? mainErr = null)
    {
        if (HasErrored(out var error, out var val))
        {
            if (mainErr is not null) 
                return mainErr + new OldResult(false, error);
            
            return error;
        }
        
        return transform(val);
    }
    
    [Pure]
    public OldTryGet<TTarget> OnSuccess<TTarget>(Func<TValue, OldTryGet<TTarget>> transform, string? mainErr = null)
    {
        if (HasErrored(out var error, out var val))
        {
            if (mainErr is not null) 
                return mainErr + new OldResult(false, error);
            
            return error;
        }

        return transform(val);
    }
}