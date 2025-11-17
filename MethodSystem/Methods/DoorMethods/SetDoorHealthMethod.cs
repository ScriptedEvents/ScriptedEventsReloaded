using LabApi.Features.Wrappers;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;

namespace SER.MethodSystem.Methods.DoorMethods;

public class SetDoorHealthMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Sets remaining health for specified doors if possible";

    public string AdditionalDescription =>
        "This is only applicable for doors that can break, like HCZ doors and not gates.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DoorsArgument("doors"),
        new FloatArgument("health")
    ];

    public override void Execute()
    {
        Door[] doors = Args.GetDoors("doors");
        float health = Args.GetFloat("health");

        doors.OfType<Door, BreakableDoor>().ForEach(door => door.Health = health);
    }
}
