using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.MapMethods;

[UsedImplicitly]
public class GetTeslasMethod : ReturningMethod<CollectionValue<ReferenceValue<Tesla>>>
{
    public override string Description => "Returns a collection of references to teslas.";

    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        ReturnValue = new(Map.Teslas);
    }
}
