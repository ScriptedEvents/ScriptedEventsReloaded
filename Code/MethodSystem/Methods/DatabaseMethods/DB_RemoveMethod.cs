using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.DatabaseMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class DB_RemoveMethod : SynchronousMethod, ICanError
{
    public override string Description => "Removes a key from a database.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DatabaseArgument("database"),
        new TextArgument("key")
    ];
    
    public override void Execute()
    {
        if (Args.GetDatabase("database").RemoveKey(Args.GetText("key")).HasErrored(out var error))
        {
            throw new ScriptRuntimeError(this, error);
        }
    }

    public string[] ErrorReasons => ["The database could not be saved."];
}
