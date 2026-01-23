using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.CollectionVariableMethods;
public class EmptyCollectionMethod : ReturningMethod<CollectionValue>
{
    public override string Description => "Returns an empty collection.";

    public override Argument[] ExpectedArguments { get; } = [];

    public override void Execute()
    {
        ReturnValue = new CollectionValue(Array.Empty<Value>());
    }
}