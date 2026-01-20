using SER.Code.Helpers.Exceptions;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;
using SER.Code.Helpers.Extensions;

namespace SER.Code.VariableSystem.Bases;

public abstract class Variable
{
    public abstract string Name { get; }

    public abstract char Prefix { get; }
    
    public abstract Value BaseValue { get; }

    public static Variable Create(string name, Value value)
    {
        return value switch
        {
            LiteralValue lit     => new LiteralVariable(name, lit),
            CollectionValue coll => new CollectionVariable(name, coll),
            PlayerValue plr      => new PlayerVariable(name, plr),
            ReferenceValue @ref  => new ReferenceVariable(name, @ref),
            _ => throw new AndrzejFuckedUpException(
                $"CreateVariable called on invalid value type {value.GetType().AccurateName}")
        };
    }
    
    public override string ToString() => $"{Prefix}{Name}";
}

public abstract class Variable<TValue> : Variable
    where TValue : Value
{
    public abstract TValue Value { get; }
    public override Value BaseValue => Value;
    
    public static implicit operator TValue(Variable<TValue> variable) => variable.Value;
}