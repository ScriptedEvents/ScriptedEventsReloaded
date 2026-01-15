using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.MethodDescriptors;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

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

        if (pickup is null) throw new ScriptRuntimeError(this, ErrorReasons[0]);
        pickup.Spawn();
        ReturnValue = pickup;
    }
}