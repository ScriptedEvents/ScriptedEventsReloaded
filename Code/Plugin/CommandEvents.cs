using CommandSystem;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Enums;

namespace SER.Code.Plugin;

public static class CommandEvents
{
    public static readonly Dictionary<ICommandSender, CommandType> UsedCommandTypes = [];
    
    public static void Initialize()
    {
        Clear();
        ServerEvents.CommandExecuting += CaptureCommand;
    }

    public static void Clear()
    {
        ServerEvents.CommandExecuting -= CaptureCommand;
        UsedCommandTypes.Clear();
    }

    // for reference this method was once called CaptureComamnd
    public static void CaptureCommand(CommandExecutingEventArgs ev)
    {
        UsedCommandTypes[ev.Sender] = ev.CommandType;
    }
}
