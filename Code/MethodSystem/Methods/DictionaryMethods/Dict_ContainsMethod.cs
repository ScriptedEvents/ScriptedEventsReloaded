using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.DictionaryMethods.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.DictionaryMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Dict_ContainsMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns true if the dictionary contains the provided key";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Dict>("dictionary"),
        new AnyValueArgument("key")
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetReference<Dict>("dictionary").ContainsKey(Args.GetAnyValue("key"));
    }
}