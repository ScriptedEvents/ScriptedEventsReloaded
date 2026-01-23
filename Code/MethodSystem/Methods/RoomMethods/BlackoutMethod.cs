using Interactables.Interobjects.DoorUtils;
using JetBrains.Annotations;
using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RoomMethods;

[UsedImplicitly]
public class BlackoutMethod : SynchronousMethod
{
    public override string Description => "Blackouts rooms.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new RoomsArgument("rooms"),
        new DurationArgument("duration")
        {
            DefaultValue = new(TimeSpan.MaxValue, "infinite")
        }
    ];
    
    public override void Execute()
    {
        var rooms = Args.GetRooms("rooms");
        var duration = Args.GetDuration("duration");
        
        var actualDuration = duration != TimeSpan.MaxValue
            ? duration.ToFloatSeconds()
            : -1;
        
        rooms.ForEach(room =>
        {
            foreach (var roomDoor in room.Doors)
            {
                roomDoor.Lock(DoorLockReason.Regular079, true);
                roomDoor.IsOpened = false;
            }

            foreach (var roomLightController in room.AllLightControllers)
            {
                roomLightController.FlickerLights(actualDuration);
            }
        });

        Timing.CallDelayed(actualDuration, () =>
        {
            foreach (var roomDoor in rooms.SelectMany(r => r.Doors))
            {
                roomDoor.IsLocked = false;
            }
        });
    }
}