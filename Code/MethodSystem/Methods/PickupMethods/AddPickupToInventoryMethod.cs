using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
public class AddPickupToInventoryMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Moves a pickup from the ground into the player's inventory.";

    public string AdditionalDescription =>
        "The pickup disappears from the ground once the player has it. " +
        "If the player's inventory is full, nothing happens and the pickup stays where it is.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        new ReferenceArgument<Pickup>("pickup")
    ];

    public override void Execute()
    {
        var player = Args.GetPlayer("player");
        var pickup = Args.GetReference<Pickup>("pickup");

        if (player.IsInventoryFull || pickup.IsDestroyed) return;

        // AddItem creates the inventory item but leaves the pickup in the world, the same as
        // the game's own pickup flow before it destroys the pickup. Remove it only once the
        // item was granted, so a failed transfer keeps the pickup.
        if (player.AddItem(pickup) is null) return;

        pickup.Destroy();
    }
}