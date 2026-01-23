using Interactables.Interobjects.DoorUtils;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ElevatorMethods;

[UsedImplicitly]
public class LockElevatorMethod : SynchronousMethod
{
    public override string Description => "Locks elevators.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ElevatorsArgument("elevators"),
        new EnumArgument<DoorLockReason>("lockReason")
    ];
    
    public override void Execute()
    {
        var elevators = Args.GetElevators("elevators");
        var lockReason = Args.GetEnum<DoorLockReason>("lockReason");
        
        elevators.ForEach(el => el.Base.ServerLockAllDoors(lockReason, true));
    }
}