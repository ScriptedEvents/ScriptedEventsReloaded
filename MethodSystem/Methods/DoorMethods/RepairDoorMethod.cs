using LabApi.Features.Wrappers;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;

namespace SER.MethodSystem.Methods.DoorMethods;

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
