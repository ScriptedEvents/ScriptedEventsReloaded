using SER.Code.Exceptions;
using SER.Code.Extensions;

namespace SER.Code.Helpers.OldResultSystem;

public readonly struct OldResult(bool wasSuccess, string errorMsg)
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

    public static implicit operator bool(OldResult result)
    {
        return result.WasSuccess;
    }

    public static implicit operator string(OldResult result)
    {
        return result.ErrorMsg;
    }

    public static implicit operator OldResult(bool res)
    {
        if (!res)
            throw new AndrzejFuckedUpException("Result cannot be returned as false without an error message.");

        return new OldResult(true, string.Empty);
    }

    public static implicit operator OldResult(string msg)
    {
        if (string.IsNullOrEmpty(msg))
            throw new AndrzejFuckedUpException("Result error message cannot be null or empty.");

        return new OldResult(false, msg);
    }

    public static OldResult Assert(bool successWhen, string errorMsg)
    {
        if (successWhen) return true;

        return errorMsg;
    }

    public static OldResult operator +(OldResult originalREs, OldResult newRes)
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

    public static OldResult Merge(params IEnumerable<OldResult> results)
    {
        return "\n" + results
            .Select(r => r.ErrorMsg)
            .Select(e => $">>> {e}")
            .JoinStrings("\n--------------------------------\n");
    }
}