using CommandSystem;
using LabApi.Features.Permissions;
using SER.Code.FlagSystem;
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
        
        ScriptFlagHandler.Clear();
        FileSystem.FileSystem.Initialize();
        
        response = "Successfully reloaded scripts. Changes in flags and script sections are now registered.";
        return true;
    }

    public string Command => "serreload";
    public string[] Aliases => [];
    public string Description => 
        "Reloads all scripts. Use after changing flags or multi-section boundaries while the server is running.";

    public string Permission => "ser.reload";
}
