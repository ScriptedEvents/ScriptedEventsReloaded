using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class ReferenceVariable(string name, ReferenceValue value) : Variable<ReferenceValue>
{
    public override string Name => name;
    public override char Prefix => '*';
    public override ReferenceValue Value => value;
}