using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.FileSystem.Structures;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.DatabaseMethods;

[UsedImplicitly]
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