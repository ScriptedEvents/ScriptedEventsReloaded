using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;
using SER.VariableSystem.Variables;

namespace SER.MethodSystem.Methods.CollectionVariableMethods;
public class CollectionInsertMethod : SynchronousMethod, IAdditionalDescription
{
    public override string? Description => "Adds the value to the collection variable";

    public string AdditionalDescription =>
        "If value is a CollectionValue, it will nest the collection inside the collection variable. " +
        "Use JoinCollections for combining collection values.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CollectionVariableArgument("collection variable"),
        new AnyValueArgument("value")
    ];

    public override void Execute()
    {
        var collVar = Args.GetCollectionVariable("collection variable");
        Script.AddVariable(new CollectionVariable(collVar.Name, collVar.Value.Insert(Args.GetAnyValue("value"))));
    }
}