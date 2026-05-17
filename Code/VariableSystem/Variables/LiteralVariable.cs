using SER.Code.Extensions;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class LiteralVariable(string name, Value value) : Variable<LiteralValue>
{
    public override string Name => name;
    public override string FriendlyName => "literal variable";
    public override Value BaseValue => value;
    
    [UsedImplicitly]
    public LiteralVariable() : this("temp", null!) {}
}

public class LiteralVariable<T>(string name, Value value) : LiteralVariable(name, value)
    where T : LiteralValue
{
    public override string FriendlyName => $"{GetFriendlyName(typeof(T))} (literal) variable";
    
    public new T ExactValue => (T) base.ExactValue;
    
    [UsedImplicitly]
    public LiteralVariable() : this("temp", null!) {}
}