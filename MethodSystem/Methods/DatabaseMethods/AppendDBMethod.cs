using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;

namespace SER.MethodSystem.Methods.DatabaseMethods;

public class AppendDBMethod : SynchronousMethod, ICanError
{
    public override string Description => "Adds a key-value pair to the database.";

    public string[] ErrorReasons =>
    [
        "Provided value cannot be stored in databases"
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new DatabaseArgument("database"),
        new TextArgument("key"),
        new AnyValueArgument("value")
        {
            Description = "For now only literal values and player values are supported."
        }
    ];

    public override void Execute()
    {
        var res = Args.GetDatabase("database").TrySet(
            Args.GetText("key"),
            Args.GetAnyValue("value")
        );

        if (res.HasErrored(out var error))
        {
            throw new ScriptRuntimeError(error);
        }
    }
}