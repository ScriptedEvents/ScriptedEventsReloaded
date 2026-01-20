using InventorySystem;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;
using Object = UnityEngine.Object;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
public class CreatePickupMethod : ReferenceReturningMethod<Pickup>
{
    public override string Description => "Creates a new item pickup to later spawn.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<ItemType>("item type"),
    ];

    public override void Execute()
    {
        var itemType = Args.GetEnum<ItemType>("item type");
        
        if (!InventoryItemLoader.AvailableItems.TryGetValue(itemType, out var prefab))
            throw new TosoksFuckedUpException("Either Northwood fucked up or you're a wizard. Congratulations!");
        
        var item = Object.Instantiate(prefab.PickupDropModel) ?? throw new TosoksFuckedUpException("Somehow the prefab failed to copy??? I don't even know who to blame tbh");
        item.Info = new(itemType, prefab.Weight);
        item.PreviousOwner = new(Server.Host?.ReferenceHub);
        
        ReturnValue = Pickup.Get(item);
    }
}