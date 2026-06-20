using System.Runtime.CompilerServices;

namespace SER.Code.ResultSystem;

public readonly struct Result
{
    private readonly ErrorList? _errors;

    // Default state (null) means Success!
    public bool IsSuccess => _errors == null;

    private Result(string? errors) => _errors = errors is not null ? new ErrorList(errors) : null;

    // Success path is entirely free (returns a struct with a null reference)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Success() => new(null);

    // Failure path allocates ONLY the string you give it
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Fail(string errorMessage) => new(errorMessage);
    
    public static implicit operator Result(ErrorList errorList) => Fail(errorList.ToString());
}