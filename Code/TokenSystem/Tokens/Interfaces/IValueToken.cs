using SER.Code.Helpers.OldResultSystem;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using ValueType = SER.Code.ValueSystem.ValueType;

namespace SER.Code.TokenSystem.Tokens.Interfaces;

public interface IValueToken
{
    /// <summary>
    /// Returns the value associated with the token.
    /// </summary>
    public OldTryGet<Value> Value();
    
    /// <summary>
    /// A signature of all possible return values.
    /// </summary>
    public ValueType PossibleValueTypes { get; }
    
    /// <summary>
    /// Whether the value is a constant and can be cached.
    /// </summary>
    public bool IsConstant { get; }
}