using Interactables.Interobjects.DoorUtils;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
public class SetDoorPermissionMethod : SynchronousMethod
{
    public override string Description => "Sets door permissions.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DoorsArgument("doors"),
        new EnumArgument<DoorPermissionFlags>("permissions")
        {
            ConsumesRemainingValues = true,
        }
    ];
    
    public override void Execute()
    {
        var doors = Args.GetDoors("doors");
        var permissions = Args
            .GetRemainingArguments<object, EnumArgument<DoorPermissionFlags>>("permissions")
            .Cast<DoorPermissionFlags>()
            .ToArray();
        
        doors.ForEach(door =>
        {
            door.Permissions = permissions.Aggregate((a, b) => a | b);
        });
    }
}