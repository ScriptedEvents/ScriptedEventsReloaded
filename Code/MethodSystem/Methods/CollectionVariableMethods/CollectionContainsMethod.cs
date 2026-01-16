using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.CollectionVariableMethods;

[UsedImplicitly]
public class CollectionContainsMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns true if the value exists in the collection";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CollectionArgument("collection"),
        new AnyValueArgument("value to check")
    ];

    public override void Execute()
    {
        var collection = Args.GetCollection("collection");
        var value = Args.GetAnyValue("value to check");
        ReturnValue = new(collection.Contains(value));
    }
}