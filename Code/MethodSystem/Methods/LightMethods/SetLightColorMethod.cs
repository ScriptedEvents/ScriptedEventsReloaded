using LabApi.Features.Wrappers;
using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.LightMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class SetLightColorMethod : SynchronousMethod
{
    public override string Description => "Sets the light color for rooms.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new RoomsArgument("rooms"),
        new ColorArgument("color"),
        new FloatArgument("intensity", 0, preferPercent: true)
        {
            DefaultValue = new(1f, "100%")
        },
        new DurationArgument("transition duration")
        {
            Description = "the amount of time in which the lights will fade to the new color",
            DefaultValue = new(null, "instant change")
        }
    ];

    public override void Execute()
    {
        var rooms = Args.GetRooms("rooms");
        var intensity = Args.GetFloat("intensity");
        var color = Args.GetColor("color") * intensity;

        // Rooms can share light controllers, so each controller is changed once.
        var controllers = rooms
            .SelectMany(room => room.AllLightControllers)
            .Distinct()
            .ToArray();

        if (Args.GetNullableDuration("transition duration") is { } duration)
        {
            // Claim the controllers now, not inside the coroutine, so a later call in the
            // same frame is always the newer one.
            var fades = controllers
                .Select(controller => new Fade(
                    controller,
                    GetStartColor(controller),
                    LightColorGenerations.Claim(controller)))
                .ToArray();

            TransitionColor(fades, color, duration.ToFloatSeconds()).Run(null);
            return;
        }

        foreach (var lightsController in controllers)
        {
            // Stops any fade still running on these lights from overwriting this color.
            LightColorGenerations.Claim(lightsController);
            lightsController.OverrideLightsColor = color;
        }
    }

    private sealed record Fade(LightsController Controller, Color StartColor, int Generation);

    private static Color GetStartColor(LightsController lightsController)
    {
        if (lightsController.OverrideLightsColor != Color.clear)
        {
            return lightsController.OverrideLightsColor;
        }

        var roomLights = lightsController.Base.transform.parent.GetComponentsInChildren<RoomLight>(true);
        return AverageColor(
            roomLights
                .Select(l => l._overrideColorSet ? l._overrideColor : l._initialLightColor)
                .Where(c => c != Color.clear)
                .ToArray());
    }

    private static IEnumerator<float> TransitionColor(Fade[] fades, Color targetColor, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            elapsedTime += Time.deltaTime;

            if (!Apply(t))
            {
                // A newer color change or reset took over every controller.
                yield break;
            }

            yield return Timing.WaitForOneFrame;
        }

        Apply(null);

        // Writes only to controllers that no newer SetLightColor or ResetLightColor has claimed.
        bool Apply(float? progress)
        {
            var anyCurrent = false;
            foreach (var fade in fades)
            {
                if (!LightColorGenerations.IsCurrent(fade.Controller, fade.Generation)
                    || fade.Controller.Base == null)
                {
                    continue;
                }

                fade.Controller.OverrideLightsColor = progress is { } value
                    ? Color.Lerp(fade.StartColor, targetColor, value)
                    : targetColor;
                anyCurrent = true;
            }

            return anyCurrent;
        }
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
