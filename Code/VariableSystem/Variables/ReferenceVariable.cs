using JetBrains.Annotations;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class ReferenceVariable(string name, ReferenceValue value) : Variable<ReferenceValue>
{
    public override string Name => name;
    public override string FriendlyName => "reference variable";
    public override ReferenceValue Value => value;
    
    [UsedImplicitly]
    public ReferenceVariable() : this("temp", null!) {}
}