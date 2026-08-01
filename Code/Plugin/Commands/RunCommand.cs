using CommandSystem;
using LabApi.Features.Permissions;
using SER.Code.FileSystem;
using SER.Code.Plugin.Commands.Interfaces;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;

namespace SER.Code.Plugin.Commands;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class RunCommand : ICommand, IUsePermissions
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasAnyPermission(Permission))
        {
            response = "You do not have permission to run scripts.";
            return false;
        }

        var name = arguments.FirstOrDefault();
        if (name is null)
        {
            response = "No script name provided. Usage: serrun <script-name>.";
            return false;
        }

        var requestedName = ScriptName.CreateUnsafe(name);
        var executor = ScriptExecutor.Get(sender);
        var creation = Script.CreateByScriptName(name, executor);

        if (creation.HasErrored(out var originalError, out var script))
        {
            var refresh = FileSystem.FileSystem.RefreshRequested(requestedName);
            if (refresh.FileFound)
            {
                creation = Script.CreateByScriptName(name, executor);
            }

            if (creation.HasErrored(out var finalError, out script))
            {
                response = BuildFailureResponse(requestedName, refresh.FileFound, originalError, finalError);
                return false;
            }
        }

        script.Run(RunReason.BaseCommand);
        response = $"Script '{script.Name}' was requested to run.";
        return true;
    }

    private static string BuildFailureResponse(
        ScriptName requestedName,
        bool fileFound,
        string originalError,
        string finalError)
    {
        if (ScriptCatalog.GetFailure(requestedName) is { } failure)
        {
            return $"SER found '{failure.Path}', but could not register it:\n{failure.Error}\n" +
                   (failure.PreviousVersionActive
                       ? "The previous accepted version remains active."
                       : "No version of this script is active.");
        }

        FileSystem.FileSystem.ParseSectionSelector(requestedName, out var fileName, out _);
        if (FileSystem.FileSystem.DuplicateScriptPaths.TryGetValue(fileName, out var conflicts))
        {
            return $"SER found multiple scripts named '{fileName}'. One name can identify only one script, " +
                   "so none of them were registered:\n" +
                   string.Join("\n", conflicts.Select(path => $"> {path}")) +
                   "\nRename all but one file, run 'serreload', and try again.";
        }

        if (!fileFound)
        {
            return $"{originalError}\nSER also searched recursively for '{fileName}.ser' and " +
                   $"'{fileName}.txt' in:\n{FileSystem.FileSystem.MainDirPath}";
        }

        return finalError;
    }

    public string Command => "serrun";
    public string[] Aliases => [];
    public string Description => "Reloads and runs the requested .ser or .txt script.";
    public string Permission => "ser.run";
}
