using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.MethodSystem.Methods.EventMethods;

[UsedImplicitly]
public class EnableEventMethod : SynchronousMethod
{
    public override string Description => "Enables the provided event to run after being disabled.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("eventName")
    ];
    
    public override void Execute()
    {
        EventHandler.EnableEvent(Args.GetText("eventName"));
    }
}