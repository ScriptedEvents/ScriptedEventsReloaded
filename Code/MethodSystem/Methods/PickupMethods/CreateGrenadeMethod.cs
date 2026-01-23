using InventorySystem;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using Object = UnityEngine.Object;
using ThrowableItem = InventorySystem.Items.ThrowableProjectiles.ThrowableItem;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
public class CreateGrenadeMethod : ReferenceReturningMethod<Projectile>, IAdditionalDescription
{
    public override string Description => "Creates a new grenade projectile to later spawn.";

    public string AdditionalDescription => 
        "To spawn SCP-018, SCP-2176 or the grenades' unactivated versions, use the Pickup method. " +
        "IMPORTANT: Northwood had a very bright idea of making the grenades not damage people when " +
        "not provided with an attacker. In order to have the grenades kill anyone, you HAVE to provide an attacker. " +
        "We know this is stupid, but for now we can't do anything about it.";

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
            throw new TosoksFuckedUpException("Either Northwood fucked up or you're a wizard. Congratulations!");
        
        var item = Object.Instantiate(throwable.Projectile) 
                   ?? throw new TosoksFuckedUpException("Somehow the prefab failed to copy??? I don't even know who to blame tbh");
        item.Info = new(itemType, throwable.Weight);
        item.PreviousOwner = new(Args.GetPlayer("attacker").ReferenceHub);
        
        ReturnValue = Projectile.Get(item);
    }
}