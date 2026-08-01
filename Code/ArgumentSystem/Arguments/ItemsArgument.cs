using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class ItemsArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => 
        $"{nameof(ItemType)} enum, " +
        $"reference to {nameof(Item)}, " +
        $"or 'all' for every item";

    [UsedImplicitly]
    public DynamicTryGet<Item[]> GetConvertSolution(BaseToken token)
    {
        if (token is SymbolToken { IsJoker: true } or AllToken)
        {
            return new(() => Item.List.ToArray());
        }

        return ValueOrEnumResolver<Item[]>(token, value =>
        {
            return value is ReferenceValue reference
                ? reference.GetAs<Item>().OnSuccess<Item[]>(item => [item])
                : GenericError(token);
        }, [
            new EnumHandler<ItemType, Item[]>(itemType => new(() => Item.GetAll(itemType).ToArray()))
        ]);
    }
}
