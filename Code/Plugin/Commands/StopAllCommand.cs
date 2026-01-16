using CommandSystem;
using LabApi.Features.Permissions;
using SER.Code.Plugin.Commands.Interfaces;
using SER.Code.ScriptSystem;

namespace SER.Code.Plugin.Commands;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class StopAllCommand : ICommand, IUsePermissions
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasAnyPermission(Permission))
        {
            response = "You do not have permission to stop scripts.";
            return false;
        }
        
        response = $"Stopped {Script.StopAll()} scripts.";
        return true;
    }

    public string Command => "serstopall";
    public string[] Aliases => [];
    public string Description => "Stops all running scripts.";
    public string Permission => "ser.stop";
}