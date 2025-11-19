using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.VariableSystem.Variables;

namespace SER.MethodSystem.Methods.CollectionVariableMethods;
public class CollectionRemoveAtMethod : SynchronousMethod
{
    public override string? Description => "Removes the value at the provided index from the collection variable";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CollectionVariableArgument("collection variable"),
        new IntArgument("index", 1)
        {
            Description = "The place in the collection to remove the value from, starting from 1"
        }
    ];

    public override void Execute()
    {
        var collVar = Args.GetCollectionVariable("collection variable");
        var index = Args.GetInt("index");

        Script.AddVariable(new CollectionVariable(collVar.Name, collVar.Value.RemoveAt(index)));
    }
}