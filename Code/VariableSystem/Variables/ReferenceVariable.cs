using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class ReferenceVariable(string name, Value value) : Variable<ReferenceValue>
{
    public override string Name => name;
    public override string FriendlyName => "reference variable";
    public override Value BaseValue => value;
    
    [UsedImplicitly]
    public ReferenceVariable() : this("temp", null!) {}
}