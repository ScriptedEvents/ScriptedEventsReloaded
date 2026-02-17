using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;
using LightSourceToy = LabApi.Features.Wrappers.LightSourceToy;

namespace SER.Code.MethodSystem.Methods.AdminToyPropertyMethods;

[UsedImplicitly]
public class SetLightSourcePropertiesMethod : SynchronousMethod
{
    public override string Description => $"Sets the properties of a {nameof(LightSourceToy)}.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<LightSourceToy>("light reference"),
        
        new FloatArgument("intensity")                { DefaultValue = new(null, "not changing") },
        new FloatArgument("range")                    { DefaultValue = new(null, "not changing") },
        new ColorArgument("color")                    { DefaultValue = new(null, "not changing") },
        new EnumArgument<LightShadows>("shadow type") { DefaultValue = new(null, "not changing") },
        new FloatArgument("shadow strength")          { DefaultValue = new(null, "not changing") },
        new EnumArgument<LightType>("light type")     { DefaultValue = new(null, "not changing") },
    ];
    public override void Execute()
    {
        var light = Args.GetReference<LightSourceToy>("light reference");
        
        if (Args.GetNullableFloat("intensity")                is { } intensity) light.Intensity = intensity;
        if (Args.GetNullableFloat("range")                    is { } range)     light.Range = range;
        if (Args.GetNullableColor("color")                    is { } color)          light.Color = color;
        if (Args.GetNullableEnum<LightShadows>("shadow type") is { } shadows)        light.ShadowType = shadows;
        if (Args.GetNullableFloat("shadow strength")          is { } strength)  light.ShadowStrength = strength;
        if (Args.GetNullableEnum<LightType>("light type")     is { } type)           light.Type = type;
    }
}