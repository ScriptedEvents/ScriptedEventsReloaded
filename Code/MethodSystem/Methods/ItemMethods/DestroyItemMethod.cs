using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class DestroyItemMethod : SynchronousMethod
{
    public override string Description => "Destroys an item in players' inventory.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new EnumArgument<ItemType>("item"),
        new IntArgument("amount", 1)
        {
            DefaultValue = new(1, null)
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var item = Args.GetEnum<ItemType>("item");
        var amount = Args.GetInt("amount");

        foreach (var plr in players)
        {
            plr.RemoveItem(item, amount);
        }
    }
}