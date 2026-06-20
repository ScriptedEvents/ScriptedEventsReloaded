using SER.Code.Exceptions;

namespace SER.Code.Helpers.OldResultSystem;

public abstract class OldDynamicTryGet
{
    public bool Static { get; protected init; }
    
    public abstract OldResult Result { get; }
    
    public static OldDynamicTryGet<string> Success(string value)
    {
        return new(new OldTryGet<string>(value, null));
    }
    
    public static OldDynamicTryGet<string> Error(string errorMsg)
    {
        return new(new OldTryGet<string>(null, errorMsg));
    }
}

public class OldDynamicTryGet<T> : OldDynamicTryGet
{
    private readonly Func<OldTryGet<T>>? _tryGetFunc;
    private readonly OldTryGet<T>? _tryGet;

    public override OldResult Result
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

    public OldTryGet<T> Invoke()
    {
        if (_tryGet is not null) return _tryGet;
        if (_tryGetFunc is not null) return _tryGetFunc();
        
        throw new InvalidOperationException();
    }
    
    public OldDynamicTryGet(T value)
    {
        Static = true;
        _tryGet = value;
    }

    public static implicit operator OldDynamicTryGet<T>(T value) => new(value);
    
    public OldDynamicTryGet(OldResult result)
    {
        Static = true;
        _tryGet = result;
    }

    public static implicit operator OldDynamicTryGet<T>(OldResult result) => new(result);


    public OldDynamicTryGet(string error)
    {
        Static = true;
        _tryGet = error;
    }

    public static implicit operator OldDynamicTryGet<T>(string error) => new(error);


    public OldDynamicTryGet(OldTryGet<T> tryGet)
    {
        Static = true;
        _tryGet = tryGet;
    }

    public static implicit operator OldDynamicTryGet<T>(OldTryGet<T> tryGet) => new(tryGet);


    public OldDynamicTryGet(Func<OldTryGet<T>> tryGetFunc)
    {
        Static = false;
        _tryGetFunc = tryGetFunc;
    }

    public static implicit operator OldDynamicTryGet<T>(Func<OldTryGet<T>> tryGetFunc) => new(tryGetFunc);
    
    public OldDynamicTryGet(Func<T> getFunc)
    {
        Static = false;
        _tryGetFunc = () => new OldTryGet<T>(getFunc(), null);
    }

    public static implicit operator OldDynamicTryGet<T>(Func<T> getFunc) => new(getFunc);
}