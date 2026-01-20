using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.WarheadMethods;

[UsedImplicitly]
public class WarheadInfoMethod : ReturningMethod
{
    public override string Description => "Returns information about Alpha Warhead";

    public override TypeOfValue Returns => new TypesOfValue([
        typeof(BoolValue),
        typeof(DurationValue)
    ]);

    public override Argument[] ExpectedArguments { get; } =
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
