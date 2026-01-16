using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
public class GetRandomDoorMethod : ReferenceReturningMethod
{
    public override string Description => "Returns a reference to a random door.";
    
    public override Type ReturnType => typeof(Door);

    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        ReturnValue = new ReferenceValue(Door.List.GetRandomValue()!);
    }
}