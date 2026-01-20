using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class CollectionVariable(string name, CollectionValue value) : Variable<CollectionValue>
{
    public override string Name => name;
    public override char Prefix => '&';
    public override CollectionValue Value => value;
}