using SER.Code.Exceptions;

namespace SER.Code.Helpers.ResultSystem;

public readonly struct Result(bool wasSuccess, string errorMsg)
{
    public readonly bool WasSuccess = wasSuccess;
    public readonly string ErrorMsg = errorMsg;

    public bool HasErrored(out string error)
    {
        error = ErrorMsg;
        return !WasSuccess;
    }

    public bool HasErrored()
    {
        return !WasSuccess;
    }

    public static implicit operator bool(Result result)
    {
        return result.WasSuccess;
    }

    public static implicit operator string(Result result)
    {
        return result.ErrorMsg;
    }

    public static implicit operator Result(bool res)
    {
        if (!res)
            throw new AndrzejFuckedUpException("Result cannot be returned as false without an error message.");

        return new Result(true, string.Empty);
    }

    public static implicit operator Result(string msg)
    {
        if (string.IsNullOrEmpty(msg))
            throw new AndrzejFuckedUpException("Result error message cannot be null or empty.");

        return new Result(false, msg);
    }

    public static Result Assert(bool successWhen, string errorMsg)
    {
        if (successWhen) return true;

        return errorMsg;
    }

    public static Result operator +(Result originalREs, Result newRes)
    {
        return new(false, $"{Process(newRes)}\n-> {Process(originalREs)}");
    }
    
    private static string Process(string value)
    {
        if (value.Length < 2) return value;
        
        if (char.IsLower(value.First()))
        {
            value = value.First().ToString().ToUpper() + value[1..];
        }

        if (!char.IsPunctuation(value.Last()))
        {
            value += ".";
        }
        
        return value;
    }
}