using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.CollectionVariableMethods;
public class JoinCollectionsMethod : ReturningMethod<CollectionValue>
{
    public override string Description => "Returns a collection that has the combined values of all the given collections";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CollectionArgument("collections")
        {
            ConsumesRemainingValues = true
        }
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetRemainingArguments<CollectionValue, CollectionArgument>("collections").Aggregate((sum, cur) => sum + cur);
    }
}