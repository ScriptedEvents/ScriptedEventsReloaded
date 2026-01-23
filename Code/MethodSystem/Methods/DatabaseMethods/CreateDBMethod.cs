using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.FileSystem.Structures;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DatabaseMethods;

[UsedImplicitly]
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