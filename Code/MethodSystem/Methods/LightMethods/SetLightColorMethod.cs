using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.LightMethods;

[UsedImplicitly]
public class SetLightColorMethod : SynchronousMethod
{
    public override string Description => "Sets the light color for rooms.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new RoomsArgument("rooms"),
        new ColorArgument("color"),
        new FloatArgument("intensity", 0)
        {
            DefaultValue = new(1f, "100%")
        }
    ];
    
    public override void Execute()
    {
        var rooms = Args.GetRooms("rooms");
        var intensity = Args.GetFloat("intensity");
        var color = Args.GetColor("color") * intensity;

        rooms.ForEachItem(room => 
            room.AllLightControllers.ForEachItem(ctrl =>
                ctrl.OverrideLightsColor = color
            )
        );
    }
}