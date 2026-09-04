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
public class OnUCRFlag : Flag, IMajorBehaviorFlag
{
    private readonly List<VariableToken> _require = [];
    private Safe<string> _event;

    public override string Description =>
        """
        Runs a script when UncomplicatedCustomRoles registers, spawns, or removes a role.
        Event values include the player, role, or active role instance when UCR provides them.
        Find supported events using the 'serhelp ucrevents' command.
        """;

    public override Argument? InlineArgument => new(
        "eventName",
        "The name of the UCR event that starts the script.",
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
            return true;
        },
        true,
        "!-- OnUCR Spawned"
    );

    public override Argument[] Arguments =>
    [
        new()
        {
            Name = "require",
            Description = "A list of variables that have to be present in order for this script to execute.",
            Handler = args =>
            {
                foreach (var arg in args)
                {
                    if (BaseToken.TryParse<VariableToken>(arg, null!).HasErrored(out var error, out var token))
                        return error;

                    _require.Add(token);
                }

                return true;
            },
            IsRequired = false,
            Example = "-- require @evPlayer *evRole"
        }
    ];

    public override Result OnScriptRunning(Script scr, out bool mustReport)
    {
        mustReport = true;
        if (base.OnScriptRunning(scr, out _).HasErrored(out var error))
            return error;

        if (scr.RunReason != RunReason.Event)
            return $"This script can only run for the UCR '{_event}' event.";

        if (_require.Any(required => scr.TryGetVariable<Variable>(required).HasErrored()))
        {
            mustReport = false;
            return "Required variable is missing. (this error should be silent, if you see it, please report it)";
        }

        return true;
    }

    public override Result Bind() => EventHandler.AddUcrEventHandler(_event, ScriptName);

    public override void Unbind() => EventHandler.RemoveUcrEventHandler(_event, ScriptName);
}
