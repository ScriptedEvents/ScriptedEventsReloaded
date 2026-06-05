using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.TimeMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Date_CurrentMethod : ReferenceReturningMethod<DateTime>
{
    public override string Description => "Returns the current date.";

    public override Argument[] ExpectedArguments { get; } = 
    [
        new OptionsArgument("time zone", "utc", "local")
        {
            DefaultValue = new("local", null)
        } 
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetOption("time zone") is "utc" ? DateTime.UtcNow : DateTime.Now;
    }
}