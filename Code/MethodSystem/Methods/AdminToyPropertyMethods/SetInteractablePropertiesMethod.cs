using AdminToys;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.AdminToyPropertyMethods;

[UsedImplicitly]
public class SetInteractablePropertiesMethod : SynchronousMethod
{
    public override string Description => "Sets properties of an Interactable Toy.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<InteractableToy>("toy"),
        new EnumArgument<InvisibleInteractableToy.ColliderShape>("shape")
        {
            DefaultValue = new(null, "not changing")
        },
        new DurationArgument("interaction duration")
        {
            DefaultValue = new(null, "not changing")
        },
        new BoolArgument("is locked")
        {
            DefaultValue = new(null, "not changing")
        }
    ];
    
    public override void Execute()
    {
        var toy = Args.GetReference<InteractableToy>("toy");
        if (Args.GetNullableEnum<InvisibleInteractableToy.ColliderShape>("shape") is { } shape)
            toy.Shape = shape;
        if (Args.GetNullableDuration("interaction duration") is { } duration)
            toy.InteractionDuration = (float)duration.TotalSeconds;
        if (Args.GetNullableBool("is locked") is { } locked)
            toy.IsLocked = locked;
    }
}