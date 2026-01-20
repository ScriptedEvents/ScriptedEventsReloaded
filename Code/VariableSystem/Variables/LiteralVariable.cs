using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;
using SER.Code.Helpers.Extensions;

namespace SER.Code.VariableSystem.Variables;

public class LiteralVariable(string name, LiteralValue value) : Variable<LiteralValue>
{
    public override string Name => name;
    public override char Prefix => '$';
    public override LiteralValue Value => value;

    public TryGet<T> TryGetValue<T>()
    {
        if (Value is T tValue)
        {
            return tValue;
        }

        return
            $"Variable '{Name}' is not a '{typeof(T).Name}' value variable, but a '{value.GetType().AccurateName}' variable.";
    }
}

public class LiteralVariable<T>(string name, T value) : LiteralVariable(name, value)
    where T : LiteralValue
{
    public new T Value => (T)base.Value;
}