using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

public class ClearInventoryMethod : SynchronousMethod, IEssential
{
    public override string Description => "Clears player inventory.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players")
    ];
    
    public override void Execute()
    {
        foreach (var plr in Args.GetPlayers("players"))
        {
            plr.Inventory.UserInventory.ReserveAmmo.Clear();
            plr.Inventory.SendAmmoNextFrame = true;
            plr.ClearInventory();
        }
    }
}