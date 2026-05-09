using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ScriptSystem;

namespace SER.Code.ArgumentSystem.BaseArguments;

public abstract class Argument(string name)
{
    public string Name { get; } = name;
    
    public bool MustBeProvided => DefaultValue is null || DefaultValue.ExplicitSkip;
    
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

    /// <summary>
    /// The default value for this argument.
    /// </summary>
    /// <param name="Value">The actual C# value that will be used. Can be null.</param>
    /// <param name="StringRep">The string representaton of the value or information regarding custom behavior.</param>
    /// <param name="ExplicitSkip">Whether the argument must be explicitly skipped using '_'</param>
    public record Default(object? Value, string? StringRep, bool ExplicitSkip = false);

    /// <summary>
    /// Sets the default value for this argument, allowing it to be skipped by the user.
    /// Null values are allowed, the method must handle it accordingly.
    /// </summary>
    public Default? DefaultValue;
    
    public abstract string InputDescription { get; }

    public Script Script { get; set; } = null!;

    public static Argument[] PlayersArgumentUpdating(
        string name, 
        Default? playerDefault = null, 
        string? playerDescription = null,
        Default? updateDefault = null,
        string? updateAdditionalDescription = null)
    {
        return
        [
            new PlayersArgument(name)
            {
                DefaultValue = playerDefault,
                Description = playerDescription
            }, 
            new BoolArgument($"update {name}")
            {
                Description = $"Whether to constantly refresh the value provided in the '{name}' argument. {updateDefault}",
                DefaultValue = updateDefault
            }
        ];
    }
}