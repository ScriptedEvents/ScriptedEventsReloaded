using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.TimeMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Date_OffsetMethod : ReferenceReturningMethod<DateTime>
{
    public override string Description => "Adds an offset to a date.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<DateTime>("date"),
        new OptionsArgument("mode", "add", "remove"),
        new DurationArgument("offset")
    ];
    
    public override void Execute()
    {
        var date = Args.GetReference<DateTime>("date");
        var offset = Args.GetDuration("offset");
        
        ReturnValue = Args.GetOption("mode") switch
        {
            "add" => date + offset,
            "remove" => date - offset,
            _ => throw new ArgumentException("Invalid mode")
        };
    }
}