using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.MethodSystem.Methods.EventMethods;

[UsedImplicitly]
public class EnableEventMethod : ReturningMethod<BoolValue>, ICanError, IAdditionalDescription
{
    public override string Description => "Enables the provided event to run after being disabled.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("eventName")
    ];
    
    public override void Execute()
    {
        if (EventHandler.EnableEvent(Args.GetText("eventName")).HasErrored(out var error, out var stateChanged))
        {
            throw new ScriptRuntimeError(this, error);
        }

        ReturnValue = stateChanged;
    }

    public string[] ErrorReasons =>
    [
        "There exists no event with the provided name.",
        "The provided event is not cancellable."
    ];

    public string AdditionalDescription =>
        "Returns true if the event was enabled, false if it was already enabled.";
}
