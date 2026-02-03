using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using CapybaraToy = LabApi.Features.Wrappers.CapybaraToy;
using LightSourceToy = LabApi.Features.Wrappers.LightSourceToy;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;
using SpeakerToy = LabApi.Features.Wrappers.SpeakerToy;
using TextToy = LabApi.Features.Wrappers.TextToy;
using WaypointToy = LabApi.Features.Wrappers.WaypointToy;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
public class CreateToyMethod : ReferenceReturningMethod<AdminToy>
{
    public override string Description => "Creates an Admin Toy";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("toy type",
            "primitiveObject",
            "lightSource",
            "shootingTarget",
            "speaker",
            "interactable",
            "camera",
            "capybara",
            "text",
            "waypoint"),
        new IntArgument("net id", 0, int.MaxValue) // not doing uint.MaxValue cuz it's obv bigger than int max
        {
            DefaultValue = new(0, "random, non-repeatable net id"),
            Description = "The maximum value is actually not the max number " +
                          "that can be placed into a net id."
        }
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetOption("toy type") switch
        {
            "primitiveobject" => PrimitiveObjectToy.Create(null, false),
            "lightsource"     => LightSourceToy.Create(null, false),
            "shootingtarget"  => ShootingTargetToy.Create(null, false),
            "speaker"         => SpeakerToy.Create(null, false),
            "interactable"    => InteractableToy.Create(null, false),
            "camera"          => CameraToy.Create(null, false),
            "capybara"        => CapybaraToy.Create(null, false),
            "text"            => TextToy.Create(null, false),
            "waypoint"        => WaypointToy.Create(null, false),
            _                 => throw new TosoksFuckedUpException("out of order")
        };
    }
}