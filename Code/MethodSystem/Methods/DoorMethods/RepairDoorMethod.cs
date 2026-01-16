using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
public class RepairDoorMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Repairs specified doors if possible";

    public string AdditionalDescription =>
        "Remember, you can't repair things like gates, but you can repair normal doors like HCZ doors";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DoorsArgument("doors to repair")
    ];

    public override void Execute()
    {
        Args.GetDoors("doors to repair")
            .OfType<Door, BreakableDoor>()
            .ForEach(door => door.TryRepair());
    }
}
