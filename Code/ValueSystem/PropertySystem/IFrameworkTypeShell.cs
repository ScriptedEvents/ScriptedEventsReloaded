using SER.Code.Helpers.ResultSystem;

namespace SER.Code.ValueSystem.PropertySystem;

/// <summary>
/// A safe, named reference wrapper around an object supplied by an optional framework.
/// </summary>
public interface IFrameworkTypeShell
{
    object Object { get; }
}

/// <summary>
/// Receives notification after SER changes a property forwarded to the wrapped object.
/// </summary>
public interface IFrameworkTypeShellWriteHandler : IFrameworkTypeShell
{
    Result OnObjectPropertySet(string propertyName);
}
