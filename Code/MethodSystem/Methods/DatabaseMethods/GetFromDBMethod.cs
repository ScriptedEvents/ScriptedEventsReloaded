using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.DatabaseMethods;

[UsedImplicitly]
public class GetFromDBMethod : ReturningMethod, ICanError
{
    public override string Description => "Returns the value of a given key in the database.";

    public override TypeOfValue Returns => new UnknownTypeOfValue();

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
            throw new ScriptRuntimeError(this, err);
        }

        ReturnValue = value;
    }
}