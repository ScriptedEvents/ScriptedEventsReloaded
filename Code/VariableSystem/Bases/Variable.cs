using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
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
        return Value.GetPrefixOfValue(value.Type) switch
        {
            '$' => new LiteralVariable(name, value),
            '&' => new CollectionVariable(name, value),
            '@' => new PlayerVariable(name, value),
            '*' => new ReferenceVariable(name, value),
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
    public TValue ExactValue => this;

    public static implicit operator TValue(Variable<TValue> variable) => variable.BaseValue switch
    {
        TValue t => t,
        IInvalidable { SafeValue: TValue t2 } => t2,
        _ => null!
    };
}