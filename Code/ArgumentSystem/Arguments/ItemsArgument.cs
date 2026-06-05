using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

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

        if (token.CanReturnReference<Item>(out var get))
        {
            return new(() => get().OnSuccess<Item[]>(item => [item]));
        }
        
        return EnumResolver<Item[]>(token, [
            new EnumHandler<ItemType, Item[]>(itemType => new(() => Item.GetAll(itemType).ToArray()))
        ]);
    }
}