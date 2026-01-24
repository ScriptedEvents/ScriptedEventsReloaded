using SER.Code.ScriptSystem;

namespace SER.Code.ArgumentSystem.BaseArguments;

public abstract class Argument(string name)
{
    public string Name { get; } = name;
    
    public bool IsRequired => DefaultValue is null;
    
    /// <summary>
    /// Allows for this argument to get an unlimited amount of values of this type
    /// Every value after this argument also counts towards this one.
    /// This argument must be the last argument of the method,
    /// </summary>
    public bool ConsumesRemainingValues { get; init; } = false;
    
    /// <summary>
    /// The short description of the argument. Use IAdditionalDescription to add more if needed.
    /// </summary>
    public string? Description { get; init; } = null;

    public record Default(object? Value, string? StringRep);

    /// <summary>
    /// Sets the default value for this argument, allowing it to be skipped by the user.
    /// Null values are allowed, the method must handle it accordingly.
    /// </summary>
    public Default? DefaultValue;
    
    public abstract string InputDescription { get; }

    public Script Script { get; set; } = null!;
}