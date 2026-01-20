using CommandSystem;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Enums;
using LabApi.Features.Permissions;
using SER.Code.Helpers.Extensions;
using SER.Code.Plugin.Commands;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.Plugin;

public static class CommandEvents
{
    public static readonly Dictionary<ICommandSender, CommandType> UsedCommandTypes = [];
    
    public static void Initialize()
    {
        UsedCommandTypes.Clear();
        ServerEvents.CommandExecuting += CaptureComamnd;
    }

    public static void CaptureComamnd(CommandExecutingEventArgs ev)
    {
        UsedCommandTypes[ev.Sender] = ev.CommandType;
        
        if (MainPlugin.Instance.Config?.MethodCommandPrefix is not true)
        {
            return;
        }
        
        if (!ev.Sender.HasPermissions(MethodCommand.RunPermission))
        {
            return;
        }

        if (!ev.CommandName.StartsWith(">"))
        {
            return;
        }
        
        var methodName = ev.CommandName.Substring(1);
        
        if (!methodName.Any())
        {
            return;
        }
        
        var script = new Script
        {
            Name = ScriptName.InitUnchecked(methodName),
            Content = $"{methodName} {ev.Arguments.JoinStrings(" ")}",
            Executor = ScriptExecutor.Get(ev.Sender, ev.CommandType)
        };

        if (Tokenizer.SliceLine(methodName).HasErrored(out _, out var slices))
        {
            return;
        }
        
        // check if the a method like this exists
        var instance = new MethodToken();
        var res = (BaseToken.IParseResult) typeof(MethodToken)
            .GetMethod(nameof(MethodToken.TryInit))!
            .Invoke(instance, [slices.First(), script, null]);

        if (res is not BaseToken.Success)
        {
            return;
        }

        ev.Sender.Respond($"Running method '{methodName}'!");
        ev.IsAllowed = false;
        script.Run();
    }
}