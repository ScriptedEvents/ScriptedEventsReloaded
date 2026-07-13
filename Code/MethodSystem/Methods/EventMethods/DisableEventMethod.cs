using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.MethodSystem.Methods.EventMethods;

[UsedImplicitly]
public class DisableEventMethod : ReturningMethod<BoolValue>, ICanError, IAdditionalDescription
{
    public override string Description => "Disables the provided event from running.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("eventName")
    ];
    
    public override void Execute()
    {
        if (EventHandler.DisableEvent(Args.GetText("eventName")).HasErrored(out var error, out var stateChanged))
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
        "Returns true if the event was disabled, false if it was already disabled.";
}
