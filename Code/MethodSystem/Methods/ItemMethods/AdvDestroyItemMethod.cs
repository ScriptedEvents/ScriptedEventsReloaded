using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

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
        var items = Args.GetItems("items");

        foreach (var item in items)
        {
            item.CurrentOwner?.RemoveItem(item);
            Pickup.Get(item.Serial)?.Destroy();
        }
    }
}