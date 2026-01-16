using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.MethodSystem.Methods.CollectionVariableMethods;

[UsedImplicitly]
public class CollectionInsertMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Adds a value to a collection variable";

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
        
        Script.AddVariable(
            new CollectionVariable(
                collVar.Name, 
                CollectionValue.Insert(collVar, Args.GetAnyValue("value"))
            )
        );
    }
}