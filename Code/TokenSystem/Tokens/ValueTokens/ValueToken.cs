using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.ResultSystem;
using SER.Code.ValueSystem;
using ValueType = SER.Code.ValueSystem.ValueType;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public abstract class ValueToken : BaseToken
{
    private bool _set = false;
    protected Value Value
    {
        get => _set ? field : throw new AndrzejFuckedUpException($"Value of a {GetType().AccurateName} was not set.");
        set
        {
            _set = true;
            field = value;
        }
    }
    
    public TryGet<Value> TryGetValue() => Value;
    public abstract ValueType ValueTypes { get; }
    public abstract bool IsConstant { get; }
}