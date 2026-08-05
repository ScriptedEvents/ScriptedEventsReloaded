using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class ForceEquipMethod : SynchronousMethod
{
    public override string Description => "Forces players to equip a provided item.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new EnumArgument<ItemType>("item type")
        {
            DefaultValue = new(ItemType.None, "Un-equip held item.")
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var itemType = Args.GetEnum<ItemType>("item type");

        if (itemType == ItemType.None)
        {
            foreach (var plr in players) plr.CurrentItem = null;
            return;
        }

        foreach (var plr in players)
        {
            var item = Item.Get(
                plr.Inventory.UserInventory.Items
                    .FirstOrDefault(x => x.Value.ItemTypeId == itemType)
                    .Value
                    .MaybeNull()
            );
            
            if (item is not null) plr.CurrentItem = item;
        }
    }
}
