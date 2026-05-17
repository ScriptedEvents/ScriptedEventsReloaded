using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Methods.DictionaryMethods.Structures;

namespace SER.Code.MethodSystem.Methods.DictionaryMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Dict_CreateMethod : ReferenceReturningMethod<Dict>, IAdditionalDescription
{
    public override string Description => "Creates an empty dictionary.";

    public string AdditionalDescription =>
        "Dictionary is a collection of unique keys (values) mapped to specific values. " +
        "Think of it like a real-world dictionary where the \"word\" is the key and the \"definition\" is the value. " +
        "E.g. you can map the value \"hello\" to the \"world\" value. " +
        "This data type is used by more experienced users, allowing for dynamic data storage.";

    public override Argument[] ExpectedArguments { get; } = [];

    public override void Execute()
    {
        ReturnValue = new Dict();
    }
}