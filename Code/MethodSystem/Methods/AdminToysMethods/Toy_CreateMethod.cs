using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.FlagSystem.Flags;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using CapybaraToy = LabApi.Features.Wrappers.CapybaraToy;
using LightSourceToy = LabApi.Features.Wrappers.LightSourceToy;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;
using TextToy = LabApi.Features.Wrappers.TextToy;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Toy_CreateMethod : ReferenceReturningMethod<AdminToy>, IAdditionalDescription
{
    public override string Description => "Creates an Admin Toy";

    public string AdditionalDescription => "Remember to also TP the created toy somewhere in order to see it.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("toy type",
            Option.Reference<PrimitiveObjectToy>("primitiveObject"),
            Option.Reference<LightSourceToy>("lightSource"),
            Option.Reference<ShootingTargetToy>("shootingTarget"), 
            Option.Reference<InteractableToy>("interactable"),
            Option.Reference<CameraToy>("camera"),
            Option.Reference<CapybaraToy>("capybara"), 
            Option.Reference<TextToy>("text")
        )
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetOption("toy type") switch
        {
            "primitiveobject" => PrimitiveObjectToy.Create(networkSpawn: false),
            "lightsource"     => LightSourceToy.Create(networkSpawn: false),
            "shootingtarget"  => ShootingTargetToy.Create(networkSpawn: false),
            "interactable"    => CreateInteractable(),
            "camera"          => CameraToy.Create(networkSpawn: false),
            "capybara"        => CapybaraToy.Create(networkSpawn: false),
            "text"            => TextToy.Create(networkSpawn: false),
            _                 => throw new TosoksFuckedUpException("out of order")
        };
    }

    public static InteractableToy CreateInteractable()
    {
        var toy = InteractableToy.Create(networkSpawn: false);
        toy.OnInteracted += plr => InteractableToyEventFlag.RunBoundScripts(plr, toy);
        return toy;
    }
}