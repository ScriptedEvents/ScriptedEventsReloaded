using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.MethodSystem.Methods.EventMethods;

[UsedImplicitly]
public class EnableEventMethod : ReturningMethod<BoolValue>, IAdditionalDescription
{
    public override string Description => "Enables the provided event to run after being disabled.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("eventName")
    ];
    
    public override void Execute()
    {
        ReturnValue = EventHandler.EnableEvent(Args.GetText("eventName"));
    }

    public string AdditionalDescription => "Returns true if the event was enabled, false if no event of this name was ever disabled.";
}