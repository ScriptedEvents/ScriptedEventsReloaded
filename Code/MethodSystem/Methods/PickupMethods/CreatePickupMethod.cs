using InventorySystem;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
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
            throw new TosoksFuckedUpException($"The pickup prefab for '{itemType}' was unavailable.");
        
        var item = Object.Instantiate(prefab.PickupDropModel)
                   ?? throw new TosoksFuckedUpException("The pickup prefab could not be instantiated.");
        item.Info = new(itemType, prefab.Weight);
        item.PreviousOwner = new(Server.Host?.ReferenceHub);
        
        ReturnValue = Pickup.Get(item);
    }
}
