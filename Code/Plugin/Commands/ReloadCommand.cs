using CommandSystem;
using LabApi.Features.Permissions;
using SER.Code.Plugin.Commands.Interfaces;

namespace SER.Code.Plugin.Commands;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class ReloadCommand : ICommand, IUsePermissions
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasAnyPermission(Permission))
        {
            response = "You do not have permission to reload scripts.";
            return false;
        }
        
        var summary = FileSystem.FileSystem.RefreshAll(true);
        response = $"Script refresh finished: {summary.Reloaded} reloaded, " +
                   $"{summary.Unloaded} unloaded, {summary.Failed} failed.";
        return summary.Failed == 0;
    }

    public string Command => "serreload";
    public string[] Aliases => [];
    public string Description => 
        "Reloads all script files from disk.";

    public string Permission => "ser.reload";
}
