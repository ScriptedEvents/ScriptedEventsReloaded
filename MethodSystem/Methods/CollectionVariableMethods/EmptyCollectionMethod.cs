using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;
using SER.Helpers.Exceptions;

namespace SER.MethodSystem.Methods.CollectionVariableMethods;
public class EmptyCollectionMethod : ReturningMethod<CollectionValue>
{
    public override string? Description => "Returns an empty collection.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("collection type",
            "bool",
            "collection",
            "duration",
            "number",
            "player",
            "reference",
            "text")
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetOption("collection type") switch
        {
            "bool" => new CollectionValue<BoolValue>([]),
            "collection" => new CollectionValue<CollectionValue>([]),
            "duration" => new CollectionValue<DurationValue>([]),
            "number" => new CollectionValue<NumberValue>([]),
            "player" => new CollectionValue<PlayerValue>([]),
            "reference" => new CollectionValue<ReferenceValue>([]),
            "text" => new CollectionValue<TextValue>([]),
            _ => throw new TosoksFuckedUpException("out of range")
        };
    }
}