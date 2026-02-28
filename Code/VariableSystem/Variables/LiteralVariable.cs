using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class LiteralVariable(string name, LiteralValue value) : Variable<LiteralValue>
{
    public override string Name => name;
    public override char Prefix => '$';
    public override string FriendlyName => "literal variable";
    public override LiteralValue Value => value;
}

public class LiteralVariable<T>(string name, T value) : LiteralVariable(name, value)
    where T : LiteralValue
{
    public new T Value => (T)base.Value;
    public override string FriendlyName => $"{typeof(T).CreateInstance<T>()} (literal) variable";
}