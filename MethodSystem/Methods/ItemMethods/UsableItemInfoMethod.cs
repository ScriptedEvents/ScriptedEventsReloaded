using LabApi.Features.Wrappers;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.ItemMethods;
internal class UsableItemInfoMethod : ReturningMethod
{
    public override string Description => "Returns information about provided usable item, like Painkillers, Medkit, etc.";

    public override Type[] ReturnTypes => [typeof(NumberValue), typeof(BoolValue)];

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<UsableItem>("usable"),
        new OptionsArgument("property",
            "useTime",
            "canUse",
            "isUsing"
            )
    ];

    public override void Execute()
    {
        UsableItem u = Args.GetReference<UsableItem>("usable");
        ReturnValue = Args.GetOption("property") switch
        {
            "usetime" => new NumberValue((decimal)u.UseDuration),
            "canuse" => new BoolValue(u.CanClientStartUsing),
            "isusing" => new BoolValue(u.IsUsing),
            _ => throw new KrzysiuFuckedUpException("out of range")
        };
    }
}
