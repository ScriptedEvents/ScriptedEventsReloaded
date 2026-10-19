using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
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
            room.AllLightControllers.ForEachItem(controller =>
            {
                // Stops any SetLightColor fade still running on these lights from
                // bringing its color back after the reset.
                LightColorGenerations.Claim(controller);
                controller.OverrideLightsColor = Color.clear;
            }));
    }
}