using Interactables.Interobjects.DoorUtils;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
public class UnlockDoorMethod : SynchronousMethod
{
    public override string Description => "Unlocks doors.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DoorsArgument("doors")
    ];

    public override void Execute()
    {
        var doors = Args.GetDoors("doors");

        foreach (var door in doors)
        {
            door.Base.NetworkActiveLocks = (ushort)DoorLockReason.None;
            DoorEvents.TriggerAction(door.Base, DoorAction.Unlocked, null);
        }
    }
}