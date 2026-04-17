using JetBrains.Annotations;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class CollectionVariable(string name, CollectionValue value) : Variable<CollectionValue>
{
    public override string Name => name;
    public override string FriendlyName => "collection variable";
    public override CollectionValue Value => value;
    
    [UsedImplicitly]
    public CollectionVariable() : this("temp", null!) {}
}