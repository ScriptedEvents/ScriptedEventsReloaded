using InventorySystem;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using Object = UnityEngine.Object;
using ThrowableItem = InventorySystem.Items.ThrowableProjectiles.ThrowableItem;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
public class CreateGrenadeMethod : ReferenceReturningMethod<Projectile>, IAdditionalDescription
{
    public override string Description => "Creates a new grenade projectile to later spawn.";

    public string AdditionalDescription => 
        "To spawn SCP-018, SCP-2176 or the grenades' unactivated versions, use the Pickup method. " +
        "Grenades require an attacker to deal damage, so this argument must identify a player.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("grenade type",
            nameof(ItemType.GrenadeHE),
            nameof(ItemType.GrenadeFlash)
        ),
        new PlayerArgument("attacker")
    ];

    public override void Execute()
    {
        if (!Enum.TryParse(Args.GetOption("grenade type"), true, out ItemType itemType) ||
            !InventoryItemLoader.TryGetItem<ThrowableItem>(itemType, out var throwable))
            throw new TosoksFuckedUpException(
                $"The projectile prefab for '{Args.GetOption("grenade type")}' was unavailable.");
        
        var item = Object.Instantiate(throwable.Projectile) 
                   ?? throw new TosoksFuckedUpException("The projectile prefab could not be instantiated.");
        item.Info = new(itemType, throwable.Weight);
        item.PreviousOwner = new(Args.GetPlayer("attacker").ReferenceHub);
        
        ReturnValue = Projectile.Get(item);
    }
}
