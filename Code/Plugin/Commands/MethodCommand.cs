using CommandSystem;
using LabApi.Features.Permissions;
using SER.Code.Plugin.Commands.Interfaces;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;

namespace SER.Code.Plugin.Commands;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class MethodCommand : ICommand, IUsePermissions
{
    public static string RunPermission => "ser.run";
    
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasAnyPermission(RunPermission))
        {
            response = "You do not have permission to run scripts.";
            return false;
        }

        var script = new Script
        {
            Name = ScriptName.InitUnchecked("Command"),
            Content = string.Join(" ", arguments.ToArray()),
            Executor = ScriptExecutor.Get(sender)
        };
        
        script.Run(RunContext.Command);
        response = "Method executed.";
        return true;
    }
    
    public string Command => "sermethod";
    public string[] Aliases => [];
    public string Description => "Runs the provied arguments at it was a line in a script.";
    public string Permission => RunPermission;
}