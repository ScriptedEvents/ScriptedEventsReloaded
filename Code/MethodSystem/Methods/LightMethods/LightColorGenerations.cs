using System.Runtime.CompilerServices;
using LabApi.Features.Wrappers;

namespace SER.Code.MethodSystem.Methods.LightMethods;

/// <summary>
/// Tracks the newest light-color change for each light controller, so a fade that is still
/// running stops writing once a newer SetLightColor or ResetLightColor targets the same lights.
/// </summary>
/// <remarks>
/// Rooms can share light controllers, so this is keyed by controller rather than by room.
/// A weak table lets controllers from earlier rounds be collected without explicit cleanup.
/// </remarks>
internal static class LightColorGenerations
{
    private sealed class Counter
    {
        public int Value;
    }

    private static readonly ConditionalWeakTable<LightsController, Counter> Counters = new();

    /// <summary>
    /// Marks a new color change for the controller. Any earlier claim stops being current.
    /// </summary>
    public static int Claim(LightsController controller)
    {
        return ++Counters.GetOrCreateValue(controller).Value;
    }

    /// <summary>
    /// Whether the claim is still the newest color change for the controller.
    /// </summary>
    public static bool IsCurrent(LightsController controller, int generation)
    {
        return Counters.TryGetValue(controller, out var counter) && counter.Value == generation;
    }
}
