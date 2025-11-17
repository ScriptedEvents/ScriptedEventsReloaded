using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.DatabaseMethods;

public class DBHasKeyMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns true if the provided key exists in the database.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DatabaseArgument("database"),
        new TextArgument("key")   
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetDatabase("database").HasKey(Args.GetText("key")).WasSuccessful();
    }
}