using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
public class PickupInfoMethod : ReturningMethod
{
    public override string Description => "Returns information about a pickup.";

    public override TypeOfValue Returns => new([
        typeof(PlayerValue),
        typeof(BoolValue),
        typeof(TextValue),
        typeof(ReferenceValue<Room>),
        typeof(NumberValue)
    ]);

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Pickup>("pickup"),
        new OptionsArgument("property",
            "isDestroyed",
            "hasSpawned", 
            Option.Enum<ItemType>(),
            "lastOwner",
            "isInUse",
            Option.Enum<ItemCategory>(),
            Option.Reference<Room>("room"),
            "positionX",
            "positionY",
            "positionZ"
        )
    ];

    public override void Execute()
    {
        var pickup = Args.GetReference<Pickup>("pickup");

        ReturnValue = Args.GetOption("property") switch
        {
            "isdestroyed" => new BoolValue(pickup.IsDestroyed),
            "hasspawned" => new BoolValue(pickup.IsSpawned),
            "itemtype" => new TextValue(pickup.Type.ToString()),
            "lastowner" => new PlayerValue(pickup.LastOwner is not null
                ? [pickup.LastOwner]
                : Array.Empty<Player>()),
            "isinuse" => new BoolValue(pickup.IsInUse),
            "itemcategory" => new TextValue(pickup.Category.ToString()),
            "room" => new ReferenceValue(pickup.Room),
            "positionx" => new NumberValue((decimal)pickup.Position.x),
            "positiony" => new NumberValue((decimal)pickup.Position.y),
            "positionz" => new NumberValue((decimal)pickup.Position.z),
            _ => throw new AndrzejFuckedUpException("out of range")
        };
    }
}