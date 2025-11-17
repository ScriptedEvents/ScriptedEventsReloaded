using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.DoorMethods;

internal class BreakDoorMethod : SynchronousMethod
{
    public override string Description => "Breaks specified doors if possible (for example, you can't destroy Gate B, but you can destroy normal HCZ doors)";

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
