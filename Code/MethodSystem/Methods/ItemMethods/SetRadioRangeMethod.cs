using InventorySystem.Items.Radio;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using RadioItem = LabApi.Features.Wrappers.RadioItem;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class SetRadioRangeMethod : SynchronousMethod
{
    public override string Description => "Sets the radio range of a specified radio item.";
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<RadioItem>("radio"),
        new EnumArgument<RadioMessages.RadioRangeLevel>("new radio range")
    ];

    public override void Execute()
    {
        Args.GetReference<RadioItem>("radio").Base._rangeId = (byte)Args.GetEnum<RadioMessages.RadioRangeLevel>("new radio range");
    }
}