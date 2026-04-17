using CommandSystem;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Enums;
using LabApi.Features.Permissions;
using SER.Code.Extensions;
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
        ServerEvents.CommandExecuting += CaptureCommand;
    }

    // for reference this method was once called CaptureComamnd
    public static void CaptureCommand(CommandExecutingEventArgs ev)
    {
        UsedCommandTypes[ev.Sender] = ev.CommandType;
    }
}