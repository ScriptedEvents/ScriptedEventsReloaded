using CommandSystem;
using LabApi.Features.Enums;
using RemoteAdmin;
using SER.Code.Exceptions;
using SER.Code.Plugin;
using Logger = LabApi.Features.Console.Logger;

namespace SER.Code.ScriptSystem.Structures;

public abstract class ScriptExecutor
{
    public abstract void Reply(string content, Script scr);
    public abstract void Warn(string content, Script scr);
    public abstract void Error(string content, Script scr);
    
    public static ScriptExecutor Get() => ServerConsoleExecutor.Instance;

    public static ScriptExecutor Get(ICommandSender sender)
    {
        if (!CommandEvents.UsedCommandTypes.TryGetValue(sender, out var type))
        {
            Logger.Warn("A command was sent, but cannot infer the command type used. Switching to server.");
            return ServerConsoleExecutor.Instance;
        }
        
        return Get(sender, type);
    }

    public static ScriptExecutor Get(ICommandSender sender, CommandType type)
    {
        if (type == CommandType.Console) return ServerConsoleExecutor.Instance;

        if (sender is not PlayerCommandSender playerSender)
        {
            Logger.Warn("A presumed command was sent, but cannot infer the player who sent it. Switching to server.");
            return ServerConsoleExecutor.Instance;
        }

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return type switch
        {
            CommandType.Client => new PlayerConsoleExecutor { Sender = playerSender.ReferenceHub },
            CommandType.RemoteAdmin => new RemoteAdminExecutor { Sender = playerSender },
            _ => throw new AndrzejFuckedUpException()
        };
    }
}