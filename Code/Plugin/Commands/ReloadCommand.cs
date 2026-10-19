using CommandSystem;
using LabApi.Features.Permissions;
using SER.Code.FileSystem;
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
        // RefreshAll rescans the folder, so this list is current.
        var conflicts = FileSystem.FileSystem.DuplicateScriptPaths;
        response = $"Script refresh finished: {summary.Reloaded} reloaded, " +
                   $"{summary.Unloaded} unloaded, {summary.Failed} failed, " +
                   $"{conflicts.Count} name conflict(s).";

        var failures = ScriptCatalog.GetFailedScripts();
        if (failures.Length > 0)
        {
            response += "\n\nFiles requiring attention:\n" +
                        string.Join("\n", failures.Select(failure =>
                            $"> {failure.Path}\n  {failure.Error}\n  " +
                            (failure.PreviousVersionActive
                                ? "The previous accepted version remains active."
                                : "No version of this script is active.")));
        }

        if (conflicts.Count > 0)
        {
            response += "\n\nName conflicts (script names must be unique, so none of these files were loaded):\n" +
                        string.Join("\n", conflicts
                            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                            .Select(pair =>
                                $"> {pair.Key}\n" + string.Join("\n", pair.Value.Select(path => $"  {path}")))) +
                        "\nRename all but one file in each group, then run 'serreload' again.";
        }

        return summary.Failed == 0 && conflicts.Count == 0;
    }

    public string Command => "serreload";
    public string[] Aliases => [];
    public string Description => "Reloads all script files and lists files with errors or name conflicts.";
    public string Permission => "ser.reload";
}
