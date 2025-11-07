using LabApi.Features.Wrappers;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;
using System;

namespace SER.MethodSystem.Methods.ItemMethods;
internal class FirearmItemInfoMethod : ReturningMethod
{
    public override string Description => "Returns info about provided firearm";

    public override Type[] ReturnTypes => [typeof(BoolValue), typeof(NumberValue)];

    public override Argument[] ExpectedArguments =>
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
