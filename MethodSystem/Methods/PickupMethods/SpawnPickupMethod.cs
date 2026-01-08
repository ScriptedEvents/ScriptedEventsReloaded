using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;
using UnityEngine;

namespace SER.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
public class SpawnPickupMethod : ReferenceReturningMethod<Pickup>, ICanError
{
    public override string Description => "Spawns an item pickup at a given position.";

    public string[] ErrorReasons =>
    [
        "Provided item cannot be spawned."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<ItemType>("item type"),
        new FloatArgument("position x"),
        new FloatArgument("position y"),
        new FloatArgument("position z"),
    ];

    public override void Execute()
    {
        var itemType = Args.GetEnum<ItemType>("item type");
        var positionX = Args.GetFloat("position x");
        var positionY = Args.GetFloat("position y");
        var positionZ = Args.GetFloat("position z");

        var pickup = Pickup.Create(
            itemType,
            new Vector3(
                positionX,
                positionY,
                positionZ
            )
        );

        if (pickup is null) throw new ScriptRuntimeError(ErrorReasons[0]);
        pickup.Spawn();
        ReturnValue = pickup;
    }
}