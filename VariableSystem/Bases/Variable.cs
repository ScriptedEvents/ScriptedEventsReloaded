using SER.Helpers.Exceptions;
using SER.Helpers.Extensions;
using SER.ValueSystem;
using SER.VariableSystem.Variables;

namespace SER.VariableSystem.Bases;

public abstract class Variable
{
    public abstract string Name { get; }

    public abstract Value BaseValue { get; }

    public static Variable CreateVariable(string name, Value value)
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
}

public abstract class Variable<TValue> : Variable
    where TValue : Value
{
    public abstract TValue Value { get; }
    public override Value BaseValue => Value;
}