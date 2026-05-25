using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.TokenSystem.Tokens.Interfaces;

public interface IValueToken
{
    /// <summary>
    /// Returns the value associated with the token.
    /// </summary>
    public TryGet<Value> Value();
    
    /// <summary>
    /// A signature of all possible return values.
    /// </summary>
    public TypeOfValue PossibleValues { get; }
    
    /// <summary>
    /// Whether the value is a constant and can be cached.
    /// </summary>
    public bool IsConstant { get; }
}