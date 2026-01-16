using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class ItemsArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => $"{nameof(ItemType)} enum, reference to {nameof(Item)}, or * for every item";

    [UsedImplicitly]
    public DynamicTryGet<Item[]> GetConvertSolution(BaseToken token)
    {
        return ResolveEnums<Item[]>(
            token,
            new()
            {
                [typeof(ItemType)] = itemType => Item.GetAll((ItemType)itemType).ToArray(),
            },
            () =>
            {
                Result rs = $"Value '{token.RawRep}' cannot be interpreted as {InputDescription}.";
                
                if (token is SymbolToken { IsJoker: true })
                {
                    return Item.List.ToArray();
                }

                if (token is not IValueToken valToken || !valToken.CanReturn<ReferenceValue>(out var get))
                {
                    return rs;
                }

                return new(() =>
                {
                    if (get().HasErrored(out var error, out var refValue))
                    {
                        return error;
                    }
                    
                    if (ReferenceArgument<Item>.TryParse(refValue).WasSuccessful(out var item))
                    {
                        return new[] { item };
                    }

                    return rs;
                });
            }
        );
    }
}