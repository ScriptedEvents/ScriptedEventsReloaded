using CommandSystem;
using LabApi.Features.Permissions;
using SER.Code.Plugin.Commands.Interfaces;
using SER.Code.ScriptSystem;

namespace SER.Code.Plugin.Commands;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class StopCommand : ICommand, IUsePermissions
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasAnyPermission(Permission))
        {
            response = "You do not have permission to stop scripts.";
            return false;
        }

        if (arguments.Count == 0)
        {
            response = "No script name provided.";
            return false;
        }

        var name = arguments.At(0);
        response = $"Stopped {Script.StopByName(name)} script(s) with name '{name}'.";
        return true;
    }

    public string Command => "serstop";
    public string[] Aliases => [];
    public string Description => "Stops all instances of a running script with a given name.";
    public string Permission => "ser.stop";
}