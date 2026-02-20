using JetBrains.Annotations;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.FlagSystem.Flags;

[UsedImplicitly]
public class OnEventFlag : Flag
{
    private Safe<string> _event;
    
    public override string Description =>
        "Binds a script to an in-game event. When the event happens, the script will execute. " +
        "Events can sometimes also carry information of their own, ";

    public override Argument? InlineArgument => new(
        "eventName",
        "The name of the event to bind to.",
        inlineArgs =>
        {
            switch (inlineArgs.Length)
            {
                case < 1:
                    return "Event name is missing";
                case > 1:
                    return "Too many arguments, only event name is allowed";
            }

            _event = inlineArgs.First();
            if (EventHandler.ConnectEvent(_event, ScriptName).HasErrored(out var error))
            {
                return error;
            }

            return true;
        },
        true
    );

    public override Result OnScriptRunning(Script scr)
    {
        if (scr.HasFlag<CustomCommandFlag>())
        {
            return $"Detected conflicting flag: {nameof(CustomCommandFlag)}.";
        }
        
        if (scr.RunReason == RunReason.Event)
        {
            return true;
        }
        
        return $"Tried to run script by other mean than the '{_event}' event, which is not allowed.";
    }

    public override Argument[] Arguments => [];

    public override void Unbind()
    {
        // done by event handler
    }
}