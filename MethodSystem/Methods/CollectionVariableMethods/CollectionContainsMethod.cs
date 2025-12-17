using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.CollectionVariableMethods;

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