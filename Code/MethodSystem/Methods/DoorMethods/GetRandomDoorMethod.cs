using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
public class GetRandomDoorMethod : ReferenceReturningMethod<Door>
{
    public override string Description => "Returns a reference to a random door.";
    
    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        ReturnValue = Door.List.GetRandomValue()!;
    }
}