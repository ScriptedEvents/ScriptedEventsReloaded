using LabApi.Features.Console;
using SER.Code.FlagSystem;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;

namespace SER.Code.FileSystem;

public static class ScriptCatalog
{
    private readonly record struct ScriptFile(string Content, DateTime LastWriteTimeUtc, long Length);
    private readonly record struct FileStamp(DateTime LastWriteTimeUtc, long Length);

    public readonly record struct RefreshSummary(int Reloaded, int Unloaded, int Failed)
    {
        public static RefreshSummary operator +(RefreshSummary first, RefreshSummary second) => new(
            first.Reloaded + second.Reloaded,
            first.Unloaded + second.Unloaded,
            first.Failed + second.Failed);
    }

    private sealed record Snapshot(
        string Path,
        string FileName,
        string RawContent,
        DateTime LastWriteTimeUtc,
        long Length,
        ScriptSection[] Sections,
        Dictionary<ScriptName, List<Flag>> Flags);

    private static readonly Dictionary<string, Snapshot> SnapshotsByPath =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Snapshot> SnapshotsByFileName =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, FileStamp> FailedFileStamps =
        new(StringComparer.OrdinalIgnoreCase);

    private static bool _initialized;
    private static bool _isRefreshing;

    public static RefreshSummary Initialize()
    {
        if (!_initialized)
        {
            _initialized = true;
            return RefreshAll(true);
        }

        foreach (string path in SnapshotsByPath.Keys.ToArray())
        {
            RestoreSnapshotBinding(path);
        }

        return default;
    }

    public static RefreshSummary RefreshAll(bool force)
    {
        if (_isRefreshing)
        {
            return default;
        }

        _isRefreshing = true;
        try
        {
            if (!Directory.Exists(FileSystem.MainDirPath))
            {
                Directory.CreateDirectory(FileSystem.MainDirPath);
            }

            try
            {
            FileSystem.UpdateScriptPathCollection();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Log.Error($"Failed to scan the SER script directory: {exception.Message}");
                return new RefreshSummary(0, 0, 1);
            }

            var paths = FileSystem.RegisteredScriptPaths.ToHashSet(StringComparer.OrdinalIgnoreCase);
            RefreshSummary summary = default;

            foreach (var removedFailure in FailedFileStamps.Keys.Where(path => !paths.Contains(path)).ToArray())
            {
                FailedFileStamps.Remove(removedFailure);
            }

            foreach (var removedPath in SnapshotsByPath.Keys.Where(path => !paths.Contains(path)).ToArray())
            {
                if (RemoveSnapshot(removedPath) is { } removedName)
                {
                    Logger.Debug($"SER script '{removedName}' was unloaded.");
                    summary += new RefreshSummary(0, 1, 0);
                }
            }

            foreach (var path in paths)
            {
                summary += RefreshPath(path, force);
            }

            return summary;
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    public static TryGet<ScriptSection> GetSection(ScriptName requestedName)
    {
        FileSystem.ParseSectionSelector(requestedName, out var fileName, out var requestedSection);
        if (!SnapshotsByFileName.TryGetValue(fileName, out var snapshot))
        {
            return $"Script '{requestedName}' does not exist anymore";
        }

        if (requestedSection is { } sectionNumber)
        {
            if (snapshot.Sections.Length <= 1)
            {
                return $"Script '{fileName}' is not split into multiple sections.";
            }

            var section = snapshot.Sections.FirstOrDefault(candidate => candidate.Number == sectionNumber);
            return section is not null
                ? section
                : $"Script '{fileName}' does not contain section {sectionNumber}.";
        }

        if (snapshot.Sections.Length > 1)
        {
            return $"Script '{fileName}' contains {snapshot.Sections.Length} sections. " +
                   $"Select one using '{fileName}:1' through '{fileName}:{snapshot.Sections.Length}'.";
        }

        return snapshot.Sections[0];
    }

    public static TryGet<string> GetPath(ScriptName requestedName)
    {
        FileSystem.ParseSectionSelector(requestedName, out var fileName, out _);
        return SnapshotsByFileName.TryGetValue(fileName, out var snapshot)
            ? TryGet<string>.Success(snapshot.Path)
            : TryGet<string>.Error($"Script '{requestedName}' does not exist anymore");
    }

    public static void Shutdown()
    {
        foreach (var snapshot in SnapshotsByPath.Values)
        {
            UnbindSnapshot(snapshot);
        }

        SnapshotsByPath.Clear();
        SnapshotsByFileName.Clear();
        FailedFileStamps.Clear();
        _initialized = false;
    }

    private static RefreshSummary RefreshPath(string path, bool force)
    {
        if (!force && TryGetFileStamp(path, out var unchangedWriteTime, out var unchangedLength))
        {
            if (FailedFileStamps.TryGetValue(path, out var failedStamp)
                && failedStamp.LastWriteTimeUtc == unchangedWriteTime
                && failedStamp.Length == unchangedLength)
            {
                return default;
            }

            if (SnapshotsByPath.TryGetValue(path, out var unchangedSnapshot)
                && unchangedSnapshot.LastWriteTimeUtc == unchangedWriteTime
                && unchangedSnapshot.Length == unchangedLength)
            {
                return default;
            }
        }

        if (ReadStableScriptFile(path).HasErrored(out var readError, out var scriptFile))
        {
            Log.CompileError(Path.GetFileNameWithoutExtension(path), readError);
            RestoreSnapshotBinding(path);
            return new RefreshSummary(0, 0, 1);
        }

        var content = scriptFile.Content;
        var lastWriteTimeUtc = scriptFile.LastWriteTimeUtc;
        var length = scriptFile.Length;

        if (!force
            && SnapshotsByPath.TryGetValue(path, out var currentSnapshot)
            && currentSnapshot.RawContent == content)
        {
            var refreshedSnapshot = currentSnapshot with
            {
                LastWriteTimeUtc = lastWriteTimeUtc,
                Length = length
            };
            SnapshotsByPath[path] = refreshedSnapshot;
            SnapshotsByFileName[refreshedSnapshot.FileName] = refreshedSnapshot;
            FailedFileStamps.Remove(path);
            return default;
        }

        if (PrepareSnapshot(path, content, lastWriteTimeUtc, length)
            .HasErrored(out var error, out var candidate))
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            FailedFileStamps[path] = new FileStamp(lastWriteTimeUtc, length);
            Log.CompileError(fileName, error);
            if (SnapshotsByPath.ContainsKey(path))
            {
                RestoreSnapshotBinding(path);
                Logger.Warn($"SER kept the last known-good version of script '{fileName}' active.");
            }

            return new RefreshSummary(0, 0, 1);
        }

        if (CommitSnapshot(candidate).HasErrored(out error))
        {
            FailedFileStamps[path] = new FileStamp(lastWriteTimeUtc, length);
            Log.CompileError(candidate.FileName, error);
            Logger.Warn($"SER kept the last known-good version of script '{candidate.FileName}' active.");
            return new RefreshSummary(0, 0, 1);
        }

        FailedFileStamps.Remove(path);
        Logger.Debug(
            $"reloaded script '{candidate.FileName}'" +
            (
                candidate.Sections.Length > 1
                    ? $" ({candidate.Sections.Length} section{(candidate.Sections.Length == 1 ? string.Empty : "s")})."
                    : string.Empty
            )
        );
        return new RefreshSummary(1, 0, 0);
    }

    private static TryGet<Snapshot> PrepareSnapshot(
        string path,
        string content,
        DateTime lastWriteTimeUtc,
        long length)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        if (ScriptSection.Split(fileName, content, path).HasErrored(out var splitError, out var sections))
        {
            return splitError;
        }

        Dictionary<ScriptName, List<Flag>> preparedFlags = [];
        foreach (var section in sections)
        {
            var script = Script.CreateByVerifiedSection(section, ServerConsoleExecutor.Instance);
            if (script.Compile().HasErrored(out var compileError))
            {
                return $"Section '{section.Name}' failed to compile: {compileError}";
            }

            if (script.GetFlagLines().HasErrored(out var flagLineError, out var flagLines))
            {
                return $"Section '{section.Name}' has invalid flags: {flagLineError}";
            }

            if (ScriptFlagHandler.PrepareScript(flagLines, section.Name)
                .HasErrored(out var flagError, out var flags))
            {
                return $"Section '{section.Name}' has invalid flags: {flagError}";
            }

            preparedFlags[section.Name] = flags;
        }

        return new Snapshot(path, fileName, content, lastWriteTimeUtc, length, sections, preparedFlags);
    }

    private static Result CommitSnapshot(Snapshot candidate)
    {
        SnapshotsByPath.TryGetValue(candidate.Path, out var previous);
        if (previous is not null)
        {
            UnbindSnapshot(previous);
        }

        List<ScriptName> boundSections = [];
        foreach (var section in candidate.Sections)
        {
            var flags = candidate.Flags[section.Name];
            if (flags.Count == 0)
            {
                continue;
            }

            if (ScriptFlagHandler.BindScript(section.Name, flags).HasErrored(out var bindError))
            {
                boundSections.ForEach(ScriptFlagHandler.UnregisterScript);
                if (previous is not null && RebindSnapshot(previous).HasErrored(out var rollbackError))
                {
                    Log.Error($"Failed to restore script '{previous.FileName}' after reload failure: {rollbackError}");
                }

                return $"Section '{section.Name}' failed to register: {bindError}";
            }

            boundSections.Add(section.Name);
        }

        SnapshotsByPath[candidate.Path] = candidate;
        RebuildFileNameIndex();
        return true;
    }

    private static Result RebindSnapshot(Snapshot snapshot)
    {
        List<ScriptName> reboundSections = [];
        foreach (var section in snapshot.Sections)
        {
            var flags = snapshot.Flags[section.Name];
            if (flags.Count > 0 && ScriptFlagHandler.BindScript(section.Name, flags).HasErrored(out var error))
            {
                reboundSections.ForEach(ScriptFlagHandler.UnregisterScript);
                return error;
            }

            if (flags.Count > 0)
            {
                reboundSections.Add(section.Name);
            }
        }

        return true;
    }

    private static void RestoreSnapshotBinding(string path)
    {
        if (!SnapshotsByPath.TryGetValue(path, out var snapshot)
            || snapshot.Flags.All(pair => pair.Value.Count == 0 || ScriptFlagHandler.ScriptsFlags.ContainsKey(pair.Key)))
        {
            return;
        }

        // Map initialization clears live bindings. Rebind the accepted in-memory snapshot
        // without rereading a file that may have changed since the last explicit reload.
        UnbindSnapshot(snapshot);
        if (RebindSnapshot(snapshot).HasErrored(out var error))
        {
            Log.Error($"Failed to restore the last known-good version of script '{snapshot.FileName}': {error}");
        }
    }

    private static void UnbindSnapshot(Snapshot snapshot)
    {
        foreach (var section in snapshot.Sections)
        {
            ScriptFlagHandler.UnregisterScript(section.Name);
        }
    }

    private static string? RemoveSnapshot(string path)
    {
        FailedFileStamps.Remove(path);
        if (!SnapshotsByPath.TryGetValue(path, out var snapshot))
        {
            return null;
        }

        UnbindSnapshot(snapshot);
        SnapshotsByPath.Remove(path);
        RebuildFileNameIndex();
        return snapshot.FileName;
    }

    private static void RebuildFileNameIndex()
    {
        SnapshotsByFileName.Clear();
        foreach (var snapshot in SnapshotsByPath.Values)
        {
            SnapshotsByFileName[snapshot.FileName] = snapshot;
        }
    }

    private static bool TryGetFileStamp(string path, out DateTime lastWriteTimeUtc, out long length)
    {
        try
        {
            var info = new FileInfo(path);
            info.Refresh();
            if (!info.Exists)
            {
                lastWriteTimeUtc = default;
                length = default;
                return false;
            }

            lastWriteTimeUtc = info.LastWriteTimeUtc;
            length = info.Length;
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            lastWriteTimeUtc = default;
            length = default;
            return false;
        }
    }

    private static TryGet<ScriptFile> ReadStableScriptFile(string path)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            if (!TryGetFileStamp(path, out var writeTimeBeforeRead, out var lengthBeforeRead))
            {
                return "Failed to inspect the script before reading it.";
            }

            string content;
            try
            {
                content = File.ReadAllText(path);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return $"Failed to read script: {exception.Message}";
            }

            if (!TryGetFileStamp(path, out var writeTimeAfterRead, out var lengthAfterRead))
            {
                return "Failed to inspect the script after reading it.";
            }

            if (writeTimeBeforeRead == writeTimeAfterRead && lengthBeforeRead == lengthAfterRead)
            {
                return new ScriptFile(content, writeTimeAfterRead, lengthAfterRead);
            }
        }

        return "The script kept changing while SER tried to read it; it will be retried on the next refresh.";
    }

}
