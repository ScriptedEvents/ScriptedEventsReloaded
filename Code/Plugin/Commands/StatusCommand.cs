using System.Text;
using CommandSystem;
using LabApi.Features.Permissions;
using SER.Code.FileSystem;
using SER.Code.Plugin.Commands.Interfaces;

namespace SER.Code.Plugin.Commands;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class StatusCommand : ICommand, IUsePermissions
{
    private const int DefaultLimit = 10;

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasAnyPermission(Permission))
        {
            response = "You do not have permission to inspect SER scripts.";
            return false;
        }

        try
        {
            FileSystem.FileSystem.UpdateScriptPathCollection(false);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            response = $"SER could not scan its script directory: {exception.Message}";
            return false;
        }

        var showAll = string.Equals(arguments.FirstOrDefault(), "all", StringComparison.OrdinalIgnoreCase);
        var limit = showAll ? int.MaxValue : DefaultLimit;
        var accepted = ScriptCatalog.GetAcceptedScripts();
        var failed = ScriptCatalog.GetFailedScripts();
        var disabled = FileSystem.FileSystem.DisabledScriptPaths;
        var disabledDirectories = FileSystem.FileSystem.DisabledScriptDirectoryPaths;
        var skippedLinks = FileSystem.FileSystem.SkippedLinkDirectoryPaths;
        var duplicates = FileSystem.FileSystem.DuplicateScriptPaths;

        var output = new StringBuilder();
        output.AppendLine("SER script status");
        output.AppendLine($"Directory: {FileSystem.FileSystem.MainDirPath}");
        output.AppendLine(
            $"Accepted: {accepted.Length} | Failed: {failed.Length} | " +
            $"Disabled by #: {disabled.Length} file(s), {disabledDirectories.Length} folder(s) | " +
            $"Skipped links: {skippedLinks.Length} | Name conflicts: {duplicates.Count}");

        AppendLimited(
            output,
            "Accepted scripts",
            accepted.Select(item =>
                $"> {item.FileName} ({item.Sections} section(s), {item.FlagBindings} flag binding(s))\n  {item.Path}"),
            limit);
        AppendLimited(
            output,
            "Failed candidates",
            failed.Select(item =>
                $"> {item.FileName}: {item.Error}\n  {item.Path}\n  " +
                (item.PreviousVersionActive
                    ? "Previous accepted version is active."
                    : "No active version.")),
            limit);
        AppendLimited(output, "Disabled files", disabled.Select(path => $"> {path}"), limit);
        AppendLimited(output, "Excluded folders", disabledDirectories.Select(path => $"> {path}"), limit);
        AppendLimited(output, "Skipped linked folders", skippedLinks.Select(path => $"> {path}"), limit);
        AppendLimited(
            output,
            "Conflicting names",
            duplicates.Select(pair =>
                $"> {pair.Key}\n" + string.Join("\n", pair.Value.Select(path => $"  {path}"))),
            limit);

        if (!showAll && new[]
                {
                    accepted.Length, failed.Length, disabled.Length, disabledDirectories.Length,
                    skippedLinks.Length, duplicates.Count
                }
                .Any(count => count > DefaultLimit))
        {
            output.AppendLine();
            output.Append("Use 'serstatus all' to show every entry.");
        }

        response = output.ToString().TrimEnd();
        return failed.Length == 0 && duplicates.Count == 0;
    }

    private static void AppendLimited(
        StringBuilder output,
        string heading,
        IEnumerable<string> entries,
        int limit)
    {
        var materialized = entries.ToArray();
        if (materialized.Length == 0)
        {
            return;
        }

        output.AppendLine();
        output.AppendLine($"{heading}:");
        foreach (var entry in materialized.Take(limit))
        {
            output.AppendLine(entry);
        }

        if (materialized.Length > limit)
        {
            output.AppendLine($"> ... {materialized.Length - limit} more");
        }
    }

    public string Command => "serstatus";
    public string[] Aliases => ["serlist"];
    public string Description => "Shows accepted, failed, disabled, excluded, linked, and conflicting scripts.";
    public string Permission => "ser.run";
}
