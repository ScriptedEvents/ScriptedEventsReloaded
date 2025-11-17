using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.CollectionVariableMethods;

public class CollectionFetchMethod : ReturningMethod
{
    public override string Description => "Returns a value from a collection variable at a given position.";
    public override Type[]? ReturnTypes => null;
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new CollectionArgument("collection"),
        new IntArgument("index", 1)
        {
            Description = "The place in the collection to fetch the value from, starting from 1"
        }
    ];
    
    public override void Execute()
    {
        var coll = Args.GetCollection("collection");
        var index = Args.GetInt("index");
        
        if (coll.GetAt(index).HasErrored(out var error, out var value))
        {
            throw new ScriptRuntimeError(error);
        }

        ReturnValue = Value.Parse(value);
    }
}