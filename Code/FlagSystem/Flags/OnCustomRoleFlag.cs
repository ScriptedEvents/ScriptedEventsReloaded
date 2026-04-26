using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.Extensions;
using SER.Code.FlagSystem.Structures;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.ScriptSystem.Structures;
using SER.Code.VariableSystem.Variables;
using Player = LabApi.Features.Wrappers.Player;

namespace SER.Code.FlagSystem.Flags;

[UsedImplicitly]
public class OnCustomRoleFlag : Flag, IMajorBehaviorFlag
{
    public override string Description => "Similar to OnEvent flag, but for custom roles.";

    private static readonly string Events = typeof(CRole.CustomRoleEvent).GetEnumNames().JoinStrings(" or ");

    public CRole.CustomRoleEvent Event { get; private set; }
    public string[]? ForRoles { get; private set; } = null;

    public override Argument? InlineArgument => new(
        "event",
        $"The event to bind to: either {Events}",
        args =>
        {
            switch (args.Length)
            {
                case < 1: return "Event name is missing.";
                case > 1: return "Too many arguments, only event name is allowed.";
            }

            if (EnumArgument<CRole.CustomRoleEvent>.ConvertOne(args[0])
                .HasErrored(out var error, out var value))
            {
                return error;
            }
            
            Event = value;
            return true;
        },
        true,
        "!-- OnCustomRole spawned"
    );

    public override Argument[] Arguments =>
    [
        new(
            "forRoles",
            "Specifies which specific roles are meant to be triggered by this script. If not provided, every role will trigger.",
            args =>
            {
                if (args.Length == 0) return "No roles specified.";
                ForRoles = args;
                return true;
            },
            false,
            "-- forRoles seniorGuard janitor"
        )
    ];

    public override void OnParsingComplete()
    {
        CRole.Handler handler = new()
        {
            Action = (plr, role) =>
            {
                if (ScriptName.GetScriptWithAutomaticLog(null) is not { } script) return;

                script.CompileWithAutomaticThrow();

                script.AddLocalVariables(
                    new PlayerVariable("evPlayer", new(plr)),
                    new ReferenceVariable("evCRole", new(role))
                );

                script.Run(RunReason.Event);
            },
            Id = ScriptName,
            ForRoles = ForRoles
        };

        CRole.EventHandlers.AddOrInitListWithKey(Event, handler);
    }

    public override void Unbind()
    {
        if (CRole.EventHandlers.TryGetValue(Event, out var list))
        {
            list.RemoveWhere(handler => handler.Id == ScriptName);
        }
    }
}