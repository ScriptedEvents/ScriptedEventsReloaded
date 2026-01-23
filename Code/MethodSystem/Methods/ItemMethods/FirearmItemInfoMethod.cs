using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ItemMethods;
internal class FirearmItemInfoMethod : ReturningMethod
{
    public override string Description => "Returns info about provided firearm";

    public override TypeOfValue Returns => new TypesOfValue([
        typeof(BoolValue),
        typeof(NumberValue)
    ]);

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<FirearmItem>("firearm"),
        new OptionsArgument("property",
            "ammo",
            "maxAmmo",
            "isCocked",
            "isMagazineInserted"
            )
    ];

    public override void Execute()
    {
        FirearmItem f = Args.GetReference<FirearmItem>("firearm");
        ReturnValue = Args.GetOption("property") switch
        {
            "ammo" => new NumberValue(f.StoredAmmo),
            "maxammo" => new NumberValue(f.MaxAmmo),
            "iscocked" => new BoolValue(f.Cocked),
            "ismagazineinserted" => new BoolValue(f.MagazineInserted),
            _ => throw new KrzysiuFuckedUpException("out of range")
        };
    }
}
