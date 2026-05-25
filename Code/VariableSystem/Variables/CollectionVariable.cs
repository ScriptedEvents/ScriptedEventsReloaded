using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class CollectionVariable(string name, Value value) : Variable<CollectionValue>
{
    public override string Name => name;
    public override string FriendlyName => "collection variable";
    public override Value BaseValue => value;
    
    [UsedImplicitly]
    public CollectionVariable() : this("temp", null!) {}
}