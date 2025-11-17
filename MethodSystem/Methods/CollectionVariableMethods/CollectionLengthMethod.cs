using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.CollectionVariableMethods;

public class CollectionLengthMethod : ReturningMethod<NumberValue>
{
    public override string? Description => "Returns the amount of items in a collection.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CollectionArgument("collection")
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetCollection("collection").CastedValues.Length;
    }
}