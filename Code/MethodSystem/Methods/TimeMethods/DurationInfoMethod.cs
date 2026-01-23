using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.TimeMethods;

public class DurationInfoMethod : ReturningMethod<NumberValue>, IAdditionalDescription
{
    public override string Description => "Returns the value of the duration as a number of specified unit.";

    public string AdditionalDescription =>
        "There is a big difference between options like 'seconds' and 'totalSeconds'. Assuming a duration of 1m " +
        "and 30s, using 'seconds' will return 30, while 'totalSeconds' will return 90.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new DurationArgument("duration"),
        new OptionsArgument("time window",
            "milliseconds",
            "totalMilliseconds",
            "seconds",
            "totalSeconds",
            "minutes",
            "totalMinutes",
            "hours",
            "totalHours"
        )
    ];

    public override void Execute()
    {
        var duration = Args.GetDuration("duration");
        ReturnValue = Args.GetOption("time window") switch
        {
            "totalmilliseconds" => (decimal)duration.TotalMilliseconds,
            "milliseconds" => duration.Milliseconds,
            "seconds" => duration.Seconds,
            "totalseconds" => (decimal)duration.TotalSeconds,
            "minutes" => duration.Minutes,
            "totalminutes" => (decimal)duration.TotalMinutes,
            "hours" => duration.Hours,
            "totalhours" => (decimal)duration.TotalHours,
            _ => throw new ArgumentException("Invalid time window option")
        };
    }
}