using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.DatabaseMethods;

[UsedImplicitly]
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