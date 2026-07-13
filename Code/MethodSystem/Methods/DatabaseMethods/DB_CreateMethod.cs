using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.FileSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.DatabaseMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class DB_CreateMethod : SynchronousMethod, ICanError
{
    public override string Description => "Creates a new JSON file in the database folder.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("name"),
    ];
    
    public override void Execute()
    {
        if (Database.Create(Args.GetText("name")).HasErrored(out var error))
        {
            throw new ScriptRuntimeError(this, error);
        }
    }

    public string[] ErrorReasons => ["The database name is invalid or resolves outside the SER database directory."];
}
