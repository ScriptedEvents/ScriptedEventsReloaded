using System.Diagnostics.CodeAnalysis;
using SER.Code.Exceptions;

namespace SER.Code.Helpers.ResultSystem;

public abstract class DynamicTryGet
{
    public bool Static { get; protected init; }
    
    public abstract Result Result { get; }
    
    public static DynamicTryGet<string> Success(string value)
    {
        return new(new TryGet<string>(value, null));
    }
    
    public static DynamicTryGet<string> Error(string errorMsg)
    {
        return new(new TryGet<string>(null, errorMsg));
    }
}

public class DynamicTryGet<T> : DynamicTryGet
{
    private readonly Func<TryGet<T>>? _tryGetFunc;
    private readonly TryGet<T>? _tryGet;

    public override Result Result
    {
        get
        {
            string? error;
            if (_tryGet is not null)
            {
                error = _tryGet.ErrorMsg;
            }
            else if (_tryGetFunc is not null)
            {
                error = _tryGetFunc().ErrorMsg;
            }
            else
            {
                throw new AndrzejFuckedUpException();
            }
            
            if (string.IsNullOrEmpty(error)) return true;
            return error!;
        }
    }

    public TryGet<T> Invoke()
    {
        if (_tryGet is not null) return _tryGet;
        if (_tryGetFunc is not null) return _tryGetFunc();
        
        return _tryGetFunc!();
    }
    
    public DynamicTryGet(T value)
    {
        Static = true;
        _tryGet = value;
    }

    public static implicit operator DynamicTryGet<T>(T value) => new(value);
    
    public DynamicTryGet(Result result)
    {
        Static = true;
        _tryGet = result;
    }

    public static implicit operator DynamicTryGet<T>(Result result) => new(result);


    public DynamicTryGet(string error)
    {
        Static = true;
        _tryGet = error;
    }

    public static implicit operator DynamicTryGet<T>(string error) => new(error);


    public DynamicTryGet(TryGet<T> tryGet)
    {
        Static = true;
        _tryGet = tryGet;
    }

    public static implicit operator DynamicTryGet<T>(TryGet<T> tryGet) => new(tryGet);


    public DynamicTryGet(Func<TryGet<T>> tryGetFunc)
    {
        Static = false;
        _tryGetFunc = tryGetFunc;
    }

    public static implicit operator DynamicTryGet<T>(Func<TryGet<T>> tryGetFunc) => new(tryGetFunc);
    
    public DynamicTryGet(Func<T> getFunc)
    {
        Static = false;
        _tryGetFunc = () => new TryGet<T>(getFunc(), null);
    }

    public static implicit operator DynamicTryGet<T>(Func<T> getFunc) => new(getFunc);
}