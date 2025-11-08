using LabApi.Features.Wrappers;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.WarheadMethods;

public class WarheadInfoMethod : ReturningMethod
{
    public override string Description => "Returns information about Alpha Warhead";

    public override Type[] ReturnTypes => [typeof(BoolValue), typeof(DurationValue)];

    public override Argument[] ExpectedArguments =>
    [
        new OptionsArgument("property",
            "isLocked",
            "isOpen",
            "isArmed",
            "hasStarted",
            "isDetonated",
            "duration"
        )
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetOption("property") switch
        {
            "islocked" => new BoolValue(Warhead.IsLocked),
            "isopen" => new BoolValue(Warhead.IsAuthorized),
            "isarmed" => new BoolValue(Warhead.LeverStatus),
            "hasstarted" => new BoolValue(Warhead.IsDetonationInProgress),
            "isdetonated" => new BoolValue(Warhead.IsDetonated),
            "duration" => new DurationValue(TimeSpan.FromSeconds(AlphaWarheadController.TimeUntilDetonation)),
            _ => throw new KrzysiuFuckedUpException()
        };
    }
}
