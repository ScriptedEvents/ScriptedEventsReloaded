using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.CollectionVariableMethods;

[UsedImplicitly]
public class CollectionLengthMethod : ReturningMethod<NumberValue>
{
    public override string Description => "Returns the amount of items in a collection.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CollectionArgument("collection")
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetCollection("collection").CastedValues.Length;
    }
}