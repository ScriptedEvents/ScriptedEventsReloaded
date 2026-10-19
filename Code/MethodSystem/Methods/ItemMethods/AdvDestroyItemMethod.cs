using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class AdvDestroyItemMethod : SynchronousMethod
{
    public override string Description => "Destroys items on the ground and in inventories.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ItemsArgument("items")
    ];
    
    public override void Execute()
    {
        var selection = Args.GetValue<ItemSelection, ItemsArgument>("items");

        // The same pickup can be reached both through an item's serial and through the
        // ground list, so each one is destroyed only once.
        HashSet<Pickup> destroyedPickups = [];

        foreach (var item in selection.Items)
        {
            item.CurrentOwner?.RemoveItem(item);
            if (Pickup.Get(item.Serial) is { } pickup)
            {
                DestroyPickup(pickup);
            }
        }

        foreach (var pickup in selection.Pickups)
        {
            DestroyPickup(pickup);
        }

        return;

        void DestroyPickup(Pickup pickup)
        {
            if (pickup.IsDestroyed || !destroyedPickups.Add(pickup)) return;
            pickup.Destroy();
        }
    }
}