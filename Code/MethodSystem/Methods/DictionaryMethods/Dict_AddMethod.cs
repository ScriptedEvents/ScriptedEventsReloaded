using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.DictionaryMethods.Structures;

namespace SER.Code.MethodSystem.Methods.DictionaryMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Dict_AddMethod : SynchronousMethod
{
    public override string Description => "Adds a key-value pair to a dictionary.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Dict>("dictionary"),
        new AnyValueArgument("key"),
        new AnyValueArgument("value")
    ];
    
    public override void Execute()
    {
        Args.GetReference<Dict>("dictionary").Add(Args.GetAnyValue("key"), Args.GetAnyValue("value"));
    }
}