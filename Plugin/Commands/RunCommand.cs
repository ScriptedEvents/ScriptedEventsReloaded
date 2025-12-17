using CommandSystem;
using LabApi.Features.Permissions;
using SER.Plugin.Commands.Interfaces;
using SER.ScriptSystem;
using SER.ScriptSystem.Structures;

namespace SER.Plugin.Commands;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class RunCommand : ICommand, IUsePermissions
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasPermissions(Permission))
        {
            response = "You do not have permission to run scripts.";
            return false;
        }
        
        var name = arguments.FirstOrDefault();
        
        if (name is null)
        {
            response = "No script name provided.";
            return false;
        }

        if (Script.CreateByScriptName(name, ScriptExecutor.Get(sender)).HasErrored(out var err, out var script))
        {
            response = err;
            return false;
        }
        
        script.Run();
        response = $"Script '{script.Name}' was requested to run";
        return true;
    }

    public string Command => "serrun";
    public string[] Aliases => [];
    public string Description => "Runs a specified script.";
    public string Permission => "ser.run";
}