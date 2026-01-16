using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.MethodSystem.Methods.CollectionVariableMethods;

[UsedImplicitly]
public class CollectionRemoveAtMethod : SynchronousMethod
{
    public override string Description => "Removes a value at the provided index from a collection variable";

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

        Script.AddVariable(
            new CollectionVariable(
                collVar.Name,
                CollectionValue.RemoveAt(collVar, index)
            )
        );
    }
}