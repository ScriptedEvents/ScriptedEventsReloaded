using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.FileSystem.Structures;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.DatabaseMethods;

public class DBExistsMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns true if the provided database exists.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("database name")
    ];
    
    public override void Execute()
    {
        ReturnValue = Database.TryGet(Args.GetText("database name")).WasSuccessful();
    }
}