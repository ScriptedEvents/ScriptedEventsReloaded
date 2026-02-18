using Interactables.Interobjects.DoorUtils;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
public class BreakDoorMethod : SynchronousMethod
{
    public override string Description => "Breaks specified doors if possible.";

    public override Argument[] ExpectedArguments { get; } = 
    [
        new DoorsArgument("doors")
        {
            Description = "Doors to break"
        },
        new EnumArgument<DoorDamageType>("damage type")
        {
            DefaultValue = new(DoorDamageType.ServerCommand, null),
            Description = "Type of damage to be applied on doors"
        }
    ];

    public override void Execute()
    {
        Door[] doors = Args.GetDoors("doors");
        var damageType = Args.GetEnum<DoorDamageType>("damage type");
        
        foreach (BreakableDoor door in doors.OfType<BreakableDoor>())
        {
            door.TryBreak(damageType);
        }
    }
}
