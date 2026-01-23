using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.CollectionVariableMethods;

[UsedImplicitly]
public class CollectionFetchMethod : ReturningMethod
{
    public override string Description => "Returns a value from a collection variable at a given position.";
    public override TypeOfValue Returns => new UnknownTypeOfValue();
    
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
            throw new ScriptRuntimeError(this, error);
        }

        ReturnValue = Value.Parse(value);
    }
}