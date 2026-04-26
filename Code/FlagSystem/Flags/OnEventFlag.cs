using JetBrains.Annotations;
using SER.Code.FlagSystem.Structures;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem.Bases;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.FlagSystem.Flags;

[UsedImplicitly]
public class OnEventFlag : Flag, IMajorBehaviorFlag
{
    private List<VariableToken> _requiredVars = [];
    private Safe<string> _event;
    
    public override string Description =>
        """
        Binds a script to an in-game event. 
        When the event happens, the script will execute. 
        Find events using 'serhelp events' command.
        """;

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
            if (EventHandler.AddEventHandler(_event, ScriptName).HasErrored(out var error))
            {
                return error;
            }

            return true;
        },
        true,
        "!-- OnEvent RoundStarted"
    );

    public override Result OnScriptRunning(Script scr, out bool mustReport)
    {
        mustReport = true;
        if (base.OnScriptRunning(scr, out _).HasErrored(out var error)) return error;

        if (scr.RunReason != RunReason.Event)
        {
            return $"Tried to run script by other mean than the '{_event}' event, which is not allowed.";
        }

        if (_requiredVars.Any(rvt => scr.TryGetVariable<Variable>(rvt).HasErrored()))
        {
            mustReport = false;
            return "Required variable is missing. (this error should be silent, if you see it, please report it)";
        }
        
        return true;
    }

    public override Argument[] Arguments => 
    [
        new()
        {
            Name = "requiredVars",
            Description = "A list of variables that have to be present in order for this script to execute.",
            Handler = args =>
            {
                foreach (var arg in args)
                {
                    if (BaseToken.TryParse<VariableToken>(arg, null!).HasErrored(out var error, out var token))
                    {
                        return error;
                    }
                    
                    _requiredVars.Add(token);
                }

                return true;
            },
            IsRequired = false,
            Example = "-- requiredVars @evPlayer *evItem"
        }
    ];

    public override void Unbind()
    {
        // done by event handler
    }
}