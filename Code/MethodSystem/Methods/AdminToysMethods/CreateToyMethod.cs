using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers.Interfaces;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using CapybaraToy = LabApi.Features.Wrappers.CapybaraToy;
using LightSourceToy = LabApi.Features.Wrappers.LightSourceToy;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;
using TextToy = LabApi.Features.Wrappers.TextToy;
using WaypointToy = LabApi.Features.Wrappers.WaypointToy;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
public class CreateToyMethod : ReferenceReturningMethod<AdminToy>, IAdditionalDescription
{
    public override string Description => "Creates an Admin Toy";

    public string AdditionalDescription =>
        $"Remember to set the position if the admin to using methods like {GetFriendlyName(typeof(TPToyPosMethod))}";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("toy type",
            Option.Reference<PrimitiveObjectToy>("primitiveObject"),
            Option.Reference<LightSourceToy>("lightSource"),
            Option.Reference<ShootingTargetToy>("shootingTarget"), 
            Option.Reference<InteractableToy>("interactable"),
            Option.Reference<CameraToy>("camera"),
            Option.Reference<CapybaraToy>("capybara"), 
            Option.Reference<TextToy>("text"),
            Option.Reference<WaypointToy>("waypoint")
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
            "waypoint"        => WaypointToy.Create(networkSpawn: false),
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