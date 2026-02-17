using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.AdminToyPropertyMethods;

[UsedImplicitly]
public class SetShootingTargetPropertiesMethod : SynchronousMethod
{
    public override string Description => $"Sets the properties of a {nameof(ShootingTargetToy)}.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<ShootingTargetToy>("target reference"),
        
        new IntArgument("max hp", 1, 256)
            { DefaultValue = new(null, "not changing") },
        new IntArgument("auto reset time", -1, 11)
            { DefaultValue = new(null, "not changing") },
        new BoolArgument("synchronize damage?")
            { DefaultValue = new(null, "not changing") },
    ];
    public override void Execute()
    {
        var target = Args.GetReference<ShootingTargetToy>("target reference");

        if (Args.GetNullableInt("max hp")          is { } hp)        target.Base._maxHp = hp;
        if (Args.GetNullableInt("auto reset time") is { } autoReset) target.Base._autoDestroyTime = autoReset;
        if (Args.GetNullableBool("sync mode")      is { } sync)    target.Base._syncMode = sync;
    }
}