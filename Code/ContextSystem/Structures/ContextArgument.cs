namespace SER.Code.ContextSystem.Structures;

/// <summary>
/// Describes one argument accepted by a keyword context.
/// This is metadata only; the context remains responsible for parsing and validating tokens.
/// </summary>
public sealed record ContextArgument(
    string Syntax,
    string Description,
    string InputDescription,
    bool IsOptional = false,
    bool ConsumesRemainingValues = false)
{
    public static implicit operator ContextArgument(string syntax) =>
        new(syntax, $"Value for {syntax}.", "See the keyword documentation for the accepted value.");

    public static ContextArgument Required(string syntax, string description, string inputDescription) =>
        new(syntax, description, inputDescription);

    public static ContextArgument Optional(string syntax, string description, string inputDescription) =>
        new(syntax, description, inputDescription, true);

    public static ContextArgument Variadic(string syntax, string description, string inputDescription) =>
        new(syntax, description, inputDescription, false, true);
}
