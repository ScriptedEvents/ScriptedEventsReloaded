using LabApi.Features.Wrappers;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.DoorMethods;

public class SetDoorMaxHealthMethod : SynchronousMethod
{
    public override string Description => "Sets max health for specified doors if possible";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DoorsArgument("doors"),
        new FloatArgument("max health")
    ];

    public override void Execute()
    {
        Door[] doors = Args.GetDoors("doors");
        float maxHp = Args.GetFloat("max health");
        
        doors.OfType<Door, BreakableDoor>().ForEach(door => door.MaxHealth = maxHp);
    }
}
