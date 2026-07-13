using CommandSystem;
using LabApi.Features.Permissions;
using SER.Code.Plugin.Commands.Interfaces;
using SER.Code.ValueSystem;
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
            Name = ScriptName.CreateUnsafe("Command"),
            Content = string.Join(" ", arguments.ToArray()),
            Executor = ScriptExecutor.Get(sender)
        };
        
        if (script.Compile().HasErrored(out var compileError))
        {
            response = compileError;
            return false;
        }

        if (script.IsSingleSynchronousReturningMethod)
        {
            if (script.RunSingleSynchronousReturningMethod(RunReason.BaseCommand)
                .HasErrored(out var runtimeError, out var result))
            {
                response = runtimeError;
                return false;
            }

            response = $"Method executed. Result: {FormatResult(result)}";
            return true;
        }

        script.Run(RunReason.BaseCommand);
        response = "Method executed.";
        return true;
    }

    private static string FormatResult(Value value) => value switch
    {
        LiteralValue literal => literal.StringRep,
        _ => value.ToString()
    };
    
    public string Command => "sermethod";
    public string[] Aliases => [];
    public string Description => "Runs the provided arguments as a single line of a script.";
    public string Permission => RunPermission;
}
