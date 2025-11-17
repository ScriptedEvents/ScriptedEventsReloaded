using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.FileSystem.Structures;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.DatabaseMethods;

public class CreateDBMethod : SynchronousMethod
{
    public override string Description => "Creates a new JSON file in the database folder.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("name"),
    ];
    
    public override void Execute()
    {
        Database.Create(Args.GetText("name"));
    }
}