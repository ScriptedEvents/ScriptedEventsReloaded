using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class ClearInventoryMethod : SynchronousMethod, IEssential
{
    public override string Description => "Clears player inventory.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new OptionsArgument("mode", "drop", "destroy")
        {
            DefaultValue = new("destroy", null)
        }
    ];
    
    public override void Execute()
    {
        if (Args.GetOption("mode") == "drop")
        {
            foreach (var plr in Args.GetPlayers("players"))
            {
                plr.DropEverything();
            }
        }
        else
        {
            foreach (var plr in Args.GetPlayers("players"))
            {
                plr.Inventory.UserInventory.ReserveAmmo.Clear();
                plr.Inventory.SendAmmoNextFrame = true;
                plr.ClearInventory();
            }
        }
    }
}