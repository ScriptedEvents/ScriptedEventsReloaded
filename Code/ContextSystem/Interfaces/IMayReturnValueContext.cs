using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Interfaces;

/// <summary>
/// Defines a context that can return a value. Used for e.g. variable definition.
/// </summary>
public interface IMayReturnValueContext
{
    /// <summary>
    /// Defines what values may be returned. Null if doesn't return anything.
    /// </summary>
    public TypeOfValue? Returns { get; }
    
    /// <summary>
    /// The returned value. Must be accessed after running the context.
    /// </summary>
    public Value? ReturnedValue { get; }
    
    /// <summary>
    /// Use when the context has failed to return a value, when <see cref="Returns"/> is not null.
    /// </summary>
    public string MissingValueHint { get; }
    
    /// <summary>
    /// Use when <see cref="Returns"></see> is null when shouldn't be.
    /// </summary>
    public string UndefinedReturnsHint { get; }
}