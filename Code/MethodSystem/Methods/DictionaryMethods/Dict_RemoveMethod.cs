using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.DictionaryMethods.Structures;

namespace SER.Code.MethodSystem.Methods.DictionaryMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Dict_RemoveMethod : SynchronousMethod
{
    public override string Description => "Removes a key-value pair from a dictionary.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Dict>("dictionary"),
        new AnyValueArgument("key to remove")
    ];
    
    public override void Execute()
    {
        Args.GetReference<Dict>("dictionary").Remove(Args.GetAnyValue("key to remove"));
    }
}