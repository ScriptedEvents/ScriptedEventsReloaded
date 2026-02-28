using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.VariableSystem.Bases;

public abstract class Variable
{
    public abstract string Name { get; }

    public abstract char Prefix { get; }
    
    public abstract Value BaseValue { get; }
    
    public abstract string FriendlyName { get; }

    public static string GetFriendlyName(Type t)
    {
        return ((Variable)t.CreateInstance()).FriendlyName;
    }
    
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
    
    public sealed override string ToString() => $"{Prefix}{Name}";
    
    public static bool AreSyntacticallySame(Variable a, Variable b) => a.Prefix == b.Prefix && a.Name == b.Name;

    public static bool AreSyntacticallySame(Variable a, VariableToken b) => a.Prefix == b.Prefix && a.Name == b.Name;
    
    public static void AssertNoVariableNameCollisions(Variable newVariable, IEnumerable<Variable> existingVariables)
    {
        if ((existingVariables as Variable[] ?? existingVariables.ToArray())
            .Any(gv => AreSyntacticallySame(gv, newVariable)))
        {
            throw new CustomScriptRuntimeError(
                $"Tried to create a variable '{newVariable}', " +
                $"but there already exists a variable with the same name."
            );
        }
    }
}

public abstract class Variable<TValue> : Variable
    where TValue : Value
{
    public abstract TValue Value { get; }
    public override Value BaseValue => Value;
    
    public static implicit operator TValue(Variable<TValue> variable) => variable.Value;
}