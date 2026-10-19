using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

/// <summary>
/// The items an <see cref="ItemsArgument"/> selected.
/// </summary>
/// <param name="Items">Items held in inventories, or the one stored item reference.</param>
/// <param name="Pickups">
/// Matching items lying on the ground. Always empty for a stored item reference, which
/// finds its own pickup by serial.
/// </param>
public sealed record ItemSelection(Item[] Items, Pickup[] Pickups);

public class ItemsArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => 
        $"{nameof(ItemType)} enum, " +
        $"reference to {nameof(Item)}, " +
        $"or 'all' for every item";

    [UsedImplicitly]
    public DynamicTryGet<ItemSelection> GetConvertSolution(BaseToken token)
    {
        if (token is SymbolToken { IsJoker: true } or AllToken)
        {
            return new(() => new ItemSelection(
                Item.List.ToArray(),
                GroundPickups().ToArray()));
        }

        return ValueOrEnumResolver<ItemSelection>(token, value =>
        {
            return value is ReferenceValue reference
                ? reference.GetAs<Item>().OnSuccess<ItemSelection>(item => new ItemSelection([item], []))
                : GenericError(token);
        }, [
            new EnumHandler<ItemType, ItemSelection>(itemType => new(() => new ItemSelection(
                Item.GetAll(itemType).ToArray(),
                GroundPickups().Where(pickup => pickup.Type == itemType).ToArray())))
        ]);
    }

    // Item.List only has items in inventories. Items lying on the ground (spawned with
    // CreatePickup, dropped, or placed by the map) exist only as pickups. Thrown grenades
    // and other projectiles in flight are left alone.
    private static IEnumerable<Pickup> GroundPickups()
    {
        return Pickup.List.Where(pickup => pickup is not Projectile && !pickup.IsDestroyed);
    }
}
