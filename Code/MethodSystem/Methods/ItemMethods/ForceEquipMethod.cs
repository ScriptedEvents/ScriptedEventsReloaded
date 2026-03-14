using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class ForceEquipMethod : SynchronousMethod
{
    public override string Description => "Forces players to equip a provided item.";

    public override Argument[] ExpectedArguments =>
        [
            new PlayersArgument("players"),
            new EnumArgument<ItemType>("item type")
                { DefaultValue = new(ItemType.None, "Un-equip held item.") }
        ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var itemType = Args.GetEnum<ItemType>("item type");
        
        players.ForEach(plr =>
        {
            var item = itemType != ItemType.None 
                ? Item.Get(plr.Inventory.UserInventory.Items.FirstOrDefault(x => x.Value.ItemTypeId == itemType).Value) 
                : null;
            plr.CurrentItem = item;
        });
    }
}