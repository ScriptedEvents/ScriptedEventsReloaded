using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class AdvGiveItemMethod : ReferenceReturningMethod<Item?>, IAdditionalDescription
{
    public override string Description => 
        "Gives a player a single item, and returns a reference to the item.";

    public string AdditionalDescription =>
        "Returned reference will be invalid if the player does not have enough space for the item.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player to give item"),
        new EnumArgument<ItemType>("item type to add")
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetPlayer("player to give item").AddItem(Args.GetEnum<ItemType>("item type to add"));
    }
}