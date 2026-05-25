using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Structures;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.VariableSystem.Bases;

public abstract class Variable : IVariableRepr
{
    public abstract string Name { get; }

    private char? _prefix;
    public char Prefix => _prefix ??= Value.GetPrefixOfValue(new(BaseValue.GetType()));
    
    public abstract Value BaseValue { get; }
    
    public abstract string FriendlyName { get; }

    // todo: replace with reflected field fetch
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
    
    public static bool AreSyntacticallySame(IVariableRepr a, IVariableRepr b) => a.Prefix == b.Prefix && a.Name == b.Name;
    
    public static void AssertNoVariableNameCollisions(Variable newVariable, IEnumerable<Variable> existingVariables)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var gv in existingVariables as Variable[] ?? existingVariables.ToArray())
        {
            if (AreSyntacticallySame(gv, newVariable))
            {
                throw new CustomScriptRuntimeError(
                    $"Tried to create a variable '{newVariable}', " + 
                    $"but there already exists a variable with the same name.");
            }
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