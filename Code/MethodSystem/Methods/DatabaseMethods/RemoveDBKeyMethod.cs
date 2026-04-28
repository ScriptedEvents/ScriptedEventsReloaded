using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DatabaseMethods;

[UsedImplicitly]
public class RemoveDBKeyMethod : SynchronousMethod
{
    public override string Description => "Removes a key from a database.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DatabaseArgument("database"),
        new TextArgument("key")
    ];
    
    public override void Execute()
    {
        Args.GetDatabase("database").RemoveKey(Args.GetText("key"));
    }
}
