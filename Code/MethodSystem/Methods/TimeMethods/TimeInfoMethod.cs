using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.TimeMethods;

[UsedImplicitly]
public class TimeInfoMethod : LiteralValueReturningMethod
{
    public override string Description => "Returns information about current time.";

    public override TypeOfValue LiteralReturnTypes => new TypesOfValue([
        typeof(NumberValue), 
        typeof(TextValue)
    ]);
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("options",
            "second",
            "minute",
            "hour",
            "month",
            "year",
            "dayOfWeek",
            new("dayOfWeekNumber", "Instead of returning e.g. 'Monday', will return 1"),
            "dayOfMonth",
            "dayOfYear",
            new("unixTimeUtc","Useful for making discord timestamps "),
            "unixTimeLocal")
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetOption("options") switch
        {
            "second" => new NumberValue(DateTime.Now.Second),
            "minute" => new NumberValue(DateTime.Now.Minute),
            "hour" => new NumberValue(DateTime.Now.Hour),
            "month" => new NumberValue(DateTime.Now.Month),
            "year" => new NumberValue(DateTime.Now.Year),
            "dayofweek" => new StaticTextValue(DateTime.Now.DayOfWeek.ToString()),
            "dayofweeknumber" => (uint)DateTime.Now.DayOfWeek == 0
                ? new NumberValue(7)
                : new NumberValue((uint)DateTime.Now.DayOfWeek),
            "dayofmonth" => new NumberValue(DateTime.Now.Day),
            "dayofyear" => new NumberValue(DateTime.Now.DayOfYear),
            "unixtimeutc" => new NumberValue(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            "unixtimelocal" => new NumberValue(DateTimeOffset.Now.ToUnixTimeSeconds()),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}