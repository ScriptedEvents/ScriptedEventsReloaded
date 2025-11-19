using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.CollectionVariableMethods;
public class JoinCollectionsMethod : ReturningMethod<CollectionValue>
{
    public override string? Description => "Returns a collection that has the combined values of all the given collections";

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