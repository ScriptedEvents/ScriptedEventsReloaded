using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.TokenSystem.Tokens.ValueTokens;

public abstract class LiteralValueToken<T> : BaseToken, IValueToken
    where T : LiteralValue 
{
    private bool _set = false;
    public T Value
    {
        get => _set ? field : throw new AndrzejFuckedUpException($"Value of a {GetType().AccurateName} was not set.");
        protected set
        {
            _set = true;
            field = value;
        }
    } = null!;
    
    OldTryGet<Value> IValueToken.Value() => Value;
    public TypeOfValue PossibleValues => new TypeOfValue<T>();
    public virtual bool IsConstant => true;
}