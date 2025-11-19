using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.VariableSystem.Variables;

namespace SER.MethodSystem.Methods.CollectionVariableMethods;
public class CollectionRemoveMethod : SynchronousMethod
{
    public override string? Description => "Removes the value from the collection variable";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CollectionVariableArgument("collection variable"),
        new AnyValueArgument("value"),
        new IntArgument("amount of matches to remove", -1)
        {
            Description = "Will delete every match if -1.",
            DefaultValue = new(-1, null)
        }
    ];

    public override void Execute()
    {
        var collVar = Args.GetCollectionVariable("collection variable");
        var i = Args.GetInt("amount of matches to remove");
        var expectedVal = Args.GetAnyValue("value");

        Script.AddVariable(new CollectionVariable(collVar.Name, collVar.Value.Remove(expectedVal, i)));
    }
}