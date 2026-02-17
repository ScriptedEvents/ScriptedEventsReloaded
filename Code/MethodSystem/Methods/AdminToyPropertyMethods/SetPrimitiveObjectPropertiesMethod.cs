using AdminToys;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace SER.Code.MethodSystem.Methods.AdminToyPropertyMethods;

[UsedImplicitly]
public class SetPrimitiveObjectPropertiesMethod : SynchronousMethod
{
    public override string Description => $"Sets properties of a {nameof(PrimitiveObjectToy)}.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<PrimitiveObjectToy>("toy reference"),
        
        new EnumArgument<PrimitiveType>("type")    { DefaultValue = new(null, "not changing") },
        new ColorArgument("color")                 { DefaultValue = new(null, "not changing") },
        new FlagsArgument<PrimitiveFlags>("flags") { DefaultValue = new(null, "not changing") },
    ];
    
    public override void Execute()
    {
        var toy = Args.GetReference<PrimitiveObjectToy>("toy reference");

        if (Args.GetNullableEnum<PrimitiveType>("type")    is { } type)  toy.Type  = type;
        if (Args.GetNullableColor("color")                 is { } color) toy.Color = color;
        if (Args.GetNullableFlags<PrimitiveFlags>("flags") is { } flags) toy.Flags = flags;
    }
}