using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ItemMethods;

[UsedImplicitly]
public class ItemInfoMethod : ReturningMethod, IReferenceResolvingMethod
{
    public override string Description => IReferenceResolvingMethod.Desc.Get(this);
    
    public Type ReferenceType => typeof(Item);

    public override TypeOfValue Returns => new TypesOfValue([
        typeof(TextValue),
        typeof(PlayerValue),
        typeof(BoolValue)
    ]);

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Item>("reference"),
        new OptionsArgument("property", 
            Option.Enum<ItemType>("type"),
            Option.Enum<ItemCategory>("category"),
            "owner",
            "isEquipped"
        )
    ];

    public override void Execute()
    {
        var item = Args.GetReference<Item>("reference");
        ReturnValue = Args.GetOption("property") switch
        {
            "type" => new TextValue(item.Type.ToString()),
            "category" => new TextValue(item.Category.ToString()),
            "owner" => new PlayerValue(item.CurrentOwner is not null ? [item.CurrentOwner] : []),
            "isequipped" => new BoolValue(item.IsEquipped),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}