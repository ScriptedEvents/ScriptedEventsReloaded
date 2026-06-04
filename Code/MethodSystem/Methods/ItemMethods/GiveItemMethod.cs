using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

public class GiveItemMethod : SynchronousMethod, IEssential
{
    public override string Description => "Gives an item to players.";

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
            for (var i = 0; i < amount; i++)
            {
                plr.AddItem(item);
            }
        }
    }
}