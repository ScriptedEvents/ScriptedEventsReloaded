using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.CollectionVariableMethods;
public class SubtractCollectionsMethod : ReturningMethod<CollectionValue>
{
    public override string Description => "Returns a collection that has the values of the first collection without the values of the latter";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CollectionArgument("original collection"),
        new CollectionArgument("collections to remove values from")
        {
            ConsumesRemainingValues = true
        }
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetRemainingArguments<CollectionValue, CollectionArgument>("collections to remove values from")
            .Aggregate(Args.GetCollection("original collection"), (sum, cur) => sum - cur);
    }
}