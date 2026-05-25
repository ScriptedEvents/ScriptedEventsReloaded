using SER.Code.Extensions;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class LiteralVariable(string name, LiteralValue value) : Variable<LiteralValue>
{
    public override string Name => name;
    public override string FriendlyName => "literal variable";
    public override LiteralValue Value => value;
    
    [UsedImplicitly]
    public LiteralVariable() : this("temp", null!) {}
}

public class LiteralVariable<T>(string name, T value) : LiteralVariable(name, value)
    where T : LiteralValue
{
    public new T Value => (T)base.Value;
    public override string FriendlyName => $"{typeof(T).CreateInstance<T>()} (literal) variable";
    
    [UsedImplicitly]
    public LiteralVariable() : this("temp", null!) {}
}