using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.EventMethods;

public class EnableEventMethod : SynchronousMethod
{
    public override string Description => "Enables the provided event to run after being disabled.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("eventName")
    ];
    
    public override void Execute()
    {
        SER.EventSystem.EventHandler.EnableEvent(Args.GetText("eventName"));
    }
}