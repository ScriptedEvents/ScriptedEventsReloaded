using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.LightMethods;

[UsedImplicitly]
public class ResetLightColorMethod : SynchronousMethod
{
    public override string Description => "Resets the light color for rooms.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new RoomsArgument("rooms")
    ];
    
    public override void Execute()
    {
        Args.GetRooms("rooms").ForEach(room => 
            room.AllLightControllers.ForEachItem(color => 
                color.OverrideLightsColor = Color.clear));
    }
}