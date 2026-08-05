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

    public readonly record struct RequestedRefreshResult(bool FileFound, RefreshSummary Summary);
    public readonly record struct AcceptedScriptInfo(
        string FileName,
        string Path,
        int Sections,
        int FlagBindings);
    public readonly record struct FailedScriptInfo(
        string FileName,
        string Path,
        string Error,
        bool PreviousVersionActive);

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
    private static readonly Dictionary<string, FailedScriptInfo> FailedScriptsByPath =
        new(StringComparer.OrdinalIgnoreCase);

    private static bool _isRefreshing;

    public static RefreshSummary Initialize()
    {
        return RefreshAll(true);
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
                foreach (var path in SnapshotsByPath.Keys.ToArray())
                {
                    RestoreSnapshotBinding(path);
                }

                return new RefreshSummary(0, 0, 1);
            }

            var paths = FileSystem.RegisteredScriptPaths.ToHashSet(StringComparer.OrdinalIgnoreCase);
            RefreshSummary summary = default;

            foreach (var removedFailure in FailedFileStamps.Keys.Where(path => !paths.Contains(path)).ToArray())
            {
                FailedFileStamps.Remove(removedFailure);
                FailedScriptsByPath.Remove(removedFailure);
            }

            foreach (var removedFailure in FailedScriptsByPath.Keys.Where(path => !paths.Contains(path)).ToArray())
            {
                FailedScriptsByPath.Remove(removedFailure);
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

    public static RequestedRefreshResult RefreshRequested(ScriptName requestedName)
    {
        if (_isRefreshing)
        {
            return default;
        }

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
            return new RequestedRefreshResult(false, new RefreshSummary(0, 0, 1));
        }

        FileSystem.ParseSectionSelector(requestedName, out var fileName, out _);
        var path = FileSystem.RegisteredScriptPaths.FirstOrDefault(candidate =>
            string.Equals(
                Path.GetFileNameWithoutExtension(candidate),
                fileName,
                StringComparison.OrdinalIgnoreCase));

        if (path is null)
        {
            if (!SnapshotsByFileName.TryGetValue(fileName, out var removedSnapshot))
            {
                return default;
            }

            _isRefreshing = true;
            try
            {
                RemoveSnapshot(removedSnapshot.Path);
                Logger.Debug($"SER script '{removedSnapshot.FileName}' was unloaded before execution.");
                return new RequestedRefreshResult(false, new RefreshSummary(0, 1, 0));
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        _isRefreshing = true;
        try
        {
            return new RequestedRefreshResult(true, RefreshPath(path, false));
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    public static AcceptedScriptInfo[] GetAcceptedScripts() => SnapshotsByPath.Values
        .OrderBy(snapshot => snapshot.FileName, StringComparer.OrdinalIgnoreCase)
        .Select(snapshot => new AcceptedScriptInfo(
            snapshot.FileName,
            snapshot.Path,
            snapshot.Sections.Length,
            snapshot.Flags.Sum(pair => pair.Value.Count)))
        .ToArray();

    public static FailedScriptInfo[] GetFailedScripts() => FailedScriptsByPath.Values
        .OrderBy(failure => failure.FileName, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public static FailedScriptInfo? GetFailure(ScriptName requestedName)
    {
        FileSystem.ParseSectionSelector(requestedName, out var fileName, out _);
        foreach (var failure in FailedScriptsByPath.Values)
        {
            if (string.Equals(failure.FileName, fileName, StringComparison.OrdinalIgnoreCase))
            {
                return failure;
            }
        }

        return null;
    }

    public static TryGet<ScriptSection> GetSection(ScriptName requestedName)
    {
        FileSystem.ParseSectionSelector(requestedName, out var fileName, out var requestedSection);
        if (!SnapshotsByFileName.TryGetValue(fileName, out var snapshot))
        {
            return $"Script '{requestedName}' is not registered.";
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

    public static TryGet<ScriptSection> GetFunctionSection(ScriptName requestedName)
    {
        FileSystem.ParseSectionSelector(requestedName, out var fileName, out var requestedSection);
        var namedFunctions = FindFunctionsByName(requestedName.ToString());
        if (namedFunctions.Length == 1)
        {
            return namedFunctions[0];
        }

        if (namedFunctions.Length > 1)
        {
            return $"Function '{requestedName}' is registered more than once. " +
                   "Function names must be globally unique.";
        }

        if (!SnapshotsByFileName.TryGetValue(fileName, out var snapshot))
        {
            return $"Function '{requestedName}' is not registered.";
        }

        if (requestedSection is not null)
        {
            if (GetSection(requestedName).HasErrored(out var sectionError, out var selectedSection))
            {
                return sectionError;
            }

            return IsFunctionSection(snapshot, selectedSection)
                ? selectedSection
                : $"Script section '{selectedSection.Name}' is not a function.";
        }

        var functionSections = snapshot.Sections
            .Where(section => IsFunctionSection(snapshot, section))
            .ToArray();

        return functionSections.Length switch
        {
            1 => functionSections[0],
            0 => $"Script '{fileName}' does not contain a Function section.",
            _ => $"Script '{fileName}' contains {functionSections.Length} Function sections. " +
                 "Select one explicitly using its section name (for example, " +
                 $"'{functionSections[0].Name}')."
        };
    }

    public static TryGet<string> GetPath(ScriptName requestedName)
    {
        FileSystem.ParseSectionSelector(requestedName, out var fileName, out _);
        return SnapshotsByFileName.TryGetValue(fileName, out var snapshot)
            ? TryGet<string>.Success(snapshot.Path)
            : TryGet<string>.Error($"Script '{requestedName}' is not registered.");
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
        FailedScriptsByPath.Clear();
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
            var fileName = Path.GetFileNameWithoutExtension(path);
            Log.CompileError(fileName, readError);
            FailedScriptsByPath[path] = new FailedScriptInfo(
                fileName,
                path,
                readError,
                SnapshotsByPath.ContainsKey(path));
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
            FailedScriptsByPath.Remove(path);
            return default;
        }

        if (PrepareSnapshot(path, content, lastWriteTimeUtc, length)
            .HasErrored(out var error, out var candidate))
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            FailedFileStamps[path] = new FileStamp(lastWriteTimeUtc, length);
            FailedScriptsByPath[path] = new FailedScriptInfo(
                fileName,
                path,
                error,
                SnapshotsByPath.ContainsKey(path));
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
            FailedScriptsByPath[path] = new FailedScriptInfo(
                candidate.FileName,
                path,
                error,
                SnapshotsByPath.ContainsKey(path));
            Log.CompileError(candidate.FileName, error);
            Logger.Warn($"SER kept the last known-good version of script '{candidate.FileName}' active.");
            return new RefreshSummary(0, 0, 1);
        }

        FailedFileStamps.Remove(path);
        FailedScriptsByPath.Remove(path);
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

        var duplicateFunctionName = preparedFlags
            .SelectMany(pair => pair.Value.OfType<FunctionFlag>())
            .Where(flag => !string.IsNullOrWhiteSpace(flag.FunctionName))
            .GroupBy(flag => flag.FunctionName!, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateFunctionName is not null)
        {
            return $"Function '{duplicateFunctionName.Key}' is defined more than once in script '{fileName}'.";
        }

        return new Snapshot(path, fileName, content, lastWriteTimeUtc, length, sections, preparedFlags);
    }

    private static Result CommitSnapshot(Snapshot candidate)
    {
        var duplicateFunctionName = candidate.Flags
            .SelectMany(pair => pair.Value.OfType<FunctionFlag>())
            .Select(flag => flag.FunctionName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .FirstOrDefault(name => FindFunctionsByName(name!)
                .Any(section => !string.Equals(section.Path, candidate.Path, StringComparison.OrdinalIgnoreCase)));
        if (duplicateFunctionName is not null)
        {
            return $"Function '{duplicateFunctionName}' is already defined in another registered script.";
        }

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
        FailedScriptsByPath.Remove(path);
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

    private static bool IsFunctionSection(Snapshot snapshot, ScriptSection section) =>
        GetFunctionFlag(snapshot, section) is not null;

    private static FunctionFlag? GetFunctionFlag(Snapshot snapshot, ScriptSection section) =>
        snapshot.Flags.TryGetValue(section.Name, out var flags)
            ? flags.OfType<FunctionFlag>().FirstOrDefault()
            : null;

    private static ScriptSection[] FindFunctionsByName(string functionName) => SnapshotsByPath.Values
        .SelectMany(snapshot => snapshot.Sections.Where(section =>
            string.Equals(
                GetFunctionFlag(snapshot, section)?.FunctionName,
                functionName,
                StringComparison.OrdinalIgnoreCase)))
        .ToArray();

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
