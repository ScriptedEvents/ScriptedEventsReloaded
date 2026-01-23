using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
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
