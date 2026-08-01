using Interactables.Interobjects.DoorUtils;
using MEC;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RoomMethods;

[UsedImplicitly]
public class BlackoutMethod : SynchronousMethod
{
    private static readonly Dictionary<Door, long> ActiveDoorLocks = [];
    private static readonly HashSet<LightsController> InfiniteBlackouts = [];
    private static readonly HashSet<CoroutineHandle> ReleaseCoroutines = [];
    private static long _nextLease;

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
        
        var doors = rooms.SelectMany(room => room.Doors).Distinct().ToArray();
        var lights = rooms.SelectMany(room => room.AllLightControllers).Distinct().ToArray();
        var lease = ++_nextLease;

        foreach (var door in doors)
        {
            door.Lock(DoorLockReason.Regular079, true);
            door.IsOpened = false;
            ActiveDoorLocks[door] = lease;
        }

        if (duration == TimeSpan.MaxValue)
        {
            foreach (var controller in lights)
            {
                controller.LightsEnabled = false;
                InfiniteBlackouts.Add(controller);
            }

            return;
        }

        var actualDuration = duration.ToFloatSeconds();
        foreach (var controller in lights)
        {
            InfiniteBlackouts.Remove(controller);
            controller.FlickerLights(actualDuration);
        }

        CoroutineHandle releaseCoroutine = default;
        releaseCoroutine = Timing.CallDelayed(actualDuration, () =>
        {
            try
            {
                ReleaseDoors(doors, lease);
            }
            finally
            {
                ReleaseCoroutines.Remove(releaseCoroutine);
            }
        });
        ReleaseCoroutines.Add(releaseCoroutine);
    }

    private static void ReleaseDoors(IEnumerable<Door> doors, long lease)
    {
        foreach (var door in doors)
        {
            if (!ActiveDoorLocks.TryGetValue(door, out var activeLease) || activeLease != lease)
            {
                continue;
            }

            ActiveDoorLocks.Remove(door);
            // Remove only the lock reason added by this method. Clearing IsLocked
            // would also remove locks owned by the game or another plugin.
            door.Lock(DoorLockReason.Regular079, false);
        }
    }

    internal static void Clear()
    {
        foreach (var coroutine in ReleaseCoroutines.ToArray())
        {
            coroutine.Kill();
        }
        ReleaseCoroutines.Clear();

        foreach (var door in ActiveDoorLocks.Keys.ToArray())
        {
            door.Lock(DoorLockReason.Regular079, false);
        }
        ActiveDoorLocks.Clear();

        foreach (var controller in InfiniteBlackouts.ToArray())
        {
            controller.LightsEnabled = true;
        }
        InfiniteBlackouts.Clear();
        _nextLease = 0;
    }
}
