using LabApi.Features.Wrappers;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;
using System;

namespace SER.MethodSystem.Methods.WarheadMethods;
public class WarheadInfoMethod : ReturningMethod
{
    public override string Description => "returns information about alpha warhead";

    public override Type[] ReturnTypes => [typeof(BoolValue), typeof(NumberValue)];

    public override Argument[] ExpectedArguments =>
    [
        new OptionsArgument("property",
            "isOpen",
            "isArmed",
            "isStarted",
            "isDetonated",
            "duration"
            )
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetOption("property") switch
        {
            "isarmed" => new BoolValue(Warhead.IsAuthorized),
            "isopen" => new BoolValue(Warhead.IsLocked),
            "isstarted" => new BoolValue(Warhead.IsDetonationInProgress),
            "isdetonated" => new BoolValue(Warhead.IsDetonated),
            "duration" => new NumberValue((decimal)AlphaWarheadController.TimeUntilDetonation),
            _ => throw new KrzysiuFuckedUpException()
        };
    }
}
