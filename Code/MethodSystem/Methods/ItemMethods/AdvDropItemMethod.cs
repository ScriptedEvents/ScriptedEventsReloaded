using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class AdvDropItemMethod : ReferenceReturningMethod
{
    public override string Description => 
        "Drops an item from player inventory and returns a reference to the pickup object of that item.";
    
    public override Type ReturnType => typeof(Pickup);

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Item>("item")
    ];

    public override void Execute()
    {
        ReturnValue = new ReferenceValue(Args.GetReference<Item>("item").DropItem());
    }
}