using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.MethodSystem.Methods.EventMethods;

[UsedImplicitly]
public class DisableEventMethod : SynchronousMethod, ICanError
{
    public override string Description => "Disables the provided event from running.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("eventName")
    ];
    
    public override void Execute()
    {
        if (EventHandler.DisableEvent(Args.GetText("eventName")).HasErrored(out var error))
        {
            throw new ScriptRuntimeError(this, error);
        }
    }

    public string[] ErrorReasons =>
    [
        "There exists no event with the provided name."
    ];
}