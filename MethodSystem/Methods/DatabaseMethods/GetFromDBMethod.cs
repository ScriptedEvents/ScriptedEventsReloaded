using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;

namespace SER.MethodSystem.Methods.DatabaseMethods;

public class GetFromDBMethod : ReturningMethod, ICanError
{
    public override string Description => "Returns the value of a given key in the database.";

    public override Type[]? ReturnTypes => null;

    public string[] ErrorReasons =>
    [
        "Provided key does not exist in the database."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new DatabaseArgument("database"),
        new TextArgument("key")
    ];

    public override void Execute()
    {
        if (Args.GetDatabase("database").Get(Args.GetText("key")).HasErrored(out var err, out var value))
        {
            throw new ScriptRuntimeError(err);
        }

        ReturnValue = value;
    }
}