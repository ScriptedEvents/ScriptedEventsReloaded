using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.LightMethods;

[UsedImplicitly]
public class TransitionLightColorMethod : SynchronousMethod
{
    public override string Description => "Transitions smoothly the light color for rooms.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new RoomsArgument("rooms"),
        new ColorArgument("color"),
        new DurationArgument("transitionDuration")
    ];
    
    public override void Execute()
    {
        var rooms = Args.GetRooms("rooms");
        var targetColor = Args.GetColor("color");
        var transitionDuration = Args.GetDuration("transitionDuration");
        
        foreach (var room in rooms)
        {
            TransitionColor(room, targetColor, transitionDuration.ToFloatSeconds()).Run(Script);
        }
    }
    
    private static IEnumerator<float> TransitionColor(Room room, Color targetColor, float duration)
    {
        Dictionary<LightsController, Color> startColor = [];
        foreach (var lightsController in room.AllLightControllers)
        {
            if (lightsController.OverrideLightsColor != Color.clear)
            {
                startColor[lightsController] = lightsController.OverrideLightsColor;
                continue;
            }
            
            var roomLights = lightsController.Base.transform.parent.GetComponentsInChildren<RoomLight>(true);
            var startColorForLight = AverageColor(
                roomLights
                    .Select(l => l._overrideColorSet ? l._overrideColor : l._initialLightColor)
                    .Where(c => c != Color.clear)
                    .ToArray());
            
            startColor[lightsController] = startColorForLight;
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            
            foreach (var lightsController in room.AllLightControllers)
            {
                Color currentColor = Color.Lerp(startColor[lightsController], targetColor, t);
                lightsController.OverrideLightsColor = currentColor;
                yield return Timing.WaitForOneFrame;
            }
        }
        
        room.AllLightControllers.ForEachItem(ctrl => ctrl.OverrideLightsColor = targetColor);
    }
    
    private static Color AverageColor(Color[] colors)
    {
        if (colors.Length == 0)
            return Color.clear;

        float r = 0f, g = 0f, b = 0f, a = 0f;

        foreach (Color color in colors)
        {
            r += color.r;
            g += color.g;
            b += color.b;
            a += color.a;
        }

        float count = colors.Length;
        return new Color(r / count, g / count, b / count, a / count);
    }
}