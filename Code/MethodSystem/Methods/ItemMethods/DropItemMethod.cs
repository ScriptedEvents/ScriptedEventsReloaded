using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class DropItemMethod : SynchronousMethod
{
    public override string Description => "Drops items from players' inventories.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new EnumArgument<ItemType>("itemTypeToDrop"),
        new IntArgument("amountToDrop", 1)
        {
            DefaultValue = new(1, null)
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var itemTypeToDrop = Args.GetEnum<ItemType>("itemTypeToDrop");
        var amountToDrop = Args.GetInt("amountToDrop");

        foreach (var plr in players)
        {
            plr.Items
                .Where(item => item.Type == itemTypeToDrop)
                .Take(amountToDrop)
                .ForEachItem(item => plr.DropItem((Item)item));
        }
    }
}