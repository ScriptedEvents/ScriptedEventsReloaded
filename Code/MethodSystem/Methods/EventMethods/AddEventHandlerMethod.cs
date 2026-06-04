using LabApi.Events.Arguments.Interfaces;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ScriptSystem.Structures;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.MethodSystem.Methods.EventMethods;

[UsedImplicitly]
public class AddEventHandlerMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Adds an event handler to the provided event.";

    public string AdditionalDescription =>
        "This is a simplified version of the event handling system. " +
        "It is recommended to use the '!-- OnEvent' flag when possible. " +
        "Additionally, the variables are NOT going to be added via the arguments defined on the function, " +
        "but automatically (like in the '!-- OnEvent' flag) - this is due to a technical limitation.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EventArgument("event name"),
        new CallbackArgument("callback"),
    ];

    public override void Execute()
    {
        var eventName = Args.GetEvent("event name");
        var callback = Args.GetCallback("callback");
        var handlerId = $"function '{callback.Name}' in script '{Script.Name}'";
        
        if (EventHandler.RegisteredHandlers.Contains(handlerId))
        {
            return;
        }
        
        var result = EventHandler.AddEventHandler(
            eventName, 
            (args, vars) => callback.Action([], scr =>
            {
                scr.AddLocalVariables(vars);
                if (args is not ICancellableEvent cancellable)
                {
                    scr.Run(RunReason.FunctionCallback);
                    return;
                }

                if (scr.RunForEvent(RunReason.FunctionCallback) is { } isAllowed)
                {
                    cancellable.IsAllowed = isAllowed;
                }
            }),
            handlerId
        );

        if (result.HasErrored(out var error))
        {
            throw new ScriptRuntimeError(this, error);
        }
    }
}