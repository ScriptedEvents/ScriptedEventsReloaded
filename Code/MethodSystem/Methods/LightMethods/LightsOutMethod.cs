using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.LightMethods;

[UsedImplicitly]
public class LightsOutMethod : SynchronousMethod
{
    public override string Description => "Turns off lights for rooms.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new RoomsArgument("rooms"),
        new DurationArgument("duration")
    ];
    
    public override void Execute()
    {
        var rooms = Args.GetRooms("rooms");
        var duration = Args.GetDuration("duration");
        
        foreach (var roomLightController in rooms.SelectMany(r => r.AllLightControllers))
        {
            roomLightController.FlickerLights(duration.ToFloatSeconds());
        }
    }
}