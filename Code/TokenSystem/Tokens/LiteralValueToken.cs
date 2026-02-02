using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens;

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
    
    TryGet<Value> IValueToken.Value() => Value;
    public TypeOfValue PossibleValues => new TypeOfValue<T>();
    public bool IsConstant => true;
}