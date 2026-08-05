using LabApi.Features.Console;
using LabApi.Loader.Features.Paths;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;

namespace SER.Code.FileSystem;

public static class FileSystem
{
    private static readonly char[] DirectorySeparators =
        [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];
    private static readonly HashSet<string> ReservedWindowsNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    public readonly record struct ExampleGenerationSummary(int Created, int AlreadyExisted, string DirectoryPath);

    public static readonly string MainDirPath = Path.Combine(PathManager.Configs.FullName, "Scripted Events Reloaded");
    public static readonly string DbDirPath = Path.Combine(MainDirPath, "Databases");
    public static readonly string ConfigsDirPath = Path.Combine(MainDirPath, "Custom Configs");
    public static string[] RegisteredScriptPaths = [];
    public static string[] DisabledScriptPaths = [];
    public static string[] DisabledScriptDirectoryPaths = [];
    public static string[] SkippedLinkDirectoryPaths = [];
    public static IReadOnlyDictionary<string, string[]> DuplicateScriptPaths { get; private set; } =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

    public static TryGet<string> GetContainedPath(string rootDirectory, string name, string extension)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return TryGet<string>.Error("A file name cannot be empty.");
        }

        try
        {
            var segments = name.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                if (segment is "." or ".."
                    || segment != segment.TrimEnd(' ', '.')
                    || segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                    || Path.DirectorySeparatorChar == '\\'
                    && ReservedWindowsNames.Contains(Path.GetFileNameWithoutExtension(segment)))
                {
                    return TryGet<string>.Error($"Path '{name}' contains an unsafe file-name segment.");
                }
            }

            var root = Path.GetFullPath(rootDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var path = Path.GetFullPath(Path.Combine(root, name + extension));
            var rootPrefix = root + Path.DirectorySeparatorChar;
            var pathComparison = Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (!path.StartsWith(rootPrefix, pathComparison))
            {
                return TryGet<string>.Error($"Path '{name}' resolves outside the SER data directory.");
            }

            var currentPath = root;
            foreach (var segment in path[rootPrefix.Length..]
                         .Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                currentPath = Path.Combine(currentPath, segment);
                if ((Directory.Exists(currentPath) || File.Exists(currentPath))
                    && (File.GetAttributes(currentPath) & FileAttributes.ReparsePoint) != 0)
                {
                    return TryGet<string>.Error($"Path '{name}' passes through a linked file or directory.");
                }
            }

            return path.AsSuccess();
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException
                                       or IOException or UnauthorizedAccessException
                                       or System.Security.SecurityException)
        {
            return TryGet<string>.Error($"Path '{name}' is invalid: {ex.Message}");
        }
    }

    public static void UpdateScriptPathCollection(bool logDuplicateErrors = true)
    {
        List<string> paths = [];
        List<string> disabledDirectories = [];
        List<string> skippedLinkDirectories = [];
        Stack<string> directories = new();
        directories.Push(MainDirPath);

        while (directories.Count > 0)
        {
            var directory = directories.Pop();
            paths.AddRange(Directory.GetFiles(directory, "*.txt", SearchOption.TopDirectoryOnly));
            paths.AddRange(Directory.GetFiles(directory, "*.ser", SearchOption.TopDirectoryOnly));

            foreach (var childDirectory in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                // Never follow links during recursive discovery. Besides allowing a script tree to
                // escape MainDirPath, directory junctions can form cycles and make refresh hang.
                if ((File.GetAttributes(childDirectory) & FileAttributes.ReparsePoint) != 0)
                {
                    skippedLinkDirectories.Add(childDirectory);
                    continue;
                }

                // A leading pound sign disables an entire directory tree, just as it disables a file.
                if (Path.GetFileName(childDirectory).StartsWith("#", StringComparison.Ordinal))
                {
                    disabledDirectories.Add(childDirectory);
                    continue;
                }

                directories.Push(childDirectory);
            }
        }

        DisabledScriptDirectoryPaths = disabledDirectories
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        SkippedLinkDirectoryPaths = skippedLinkDirectories
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        DisabledScriptPaths = paths
            .Where(path => Path.GetFileName(path).StartsWith("#", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        RegisteredScriptPaths = paths
            // Ignore files with a pound sign at the start.
            .Where(path => !Path.GetFileName(path).StartsWith("#", StringComparison.Ordinal))
            .ToArray();

        DuplicateScriptPaths = RegisteredScriptPaths
            .GroupBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        foreach (var duplicate in DuplicateScriptPaths.Where(_ => logDuplicateErrors))
        {
            Logger.Error(
                $"SER found {duplicate.Value.Length} scripts named '{duplicate.Key}'. " +
                "Script names must be globally unique, so none of these files were loaded:\n" +
                string.Join("\n", duplicate.Value.Select(path => $"> {path}")) +
                "\nRename all but one of these files, then run 'serreload'."
            );
        }

        if (DuplicateScriptPaths.Count == 0) return;

        RegisteredScriptPaths = RegisteredScriptPaths
            .Where(path => !DuplicateScriptPaths.ContainsKey(Path.GetFileNameWithoutExtension(path)))
            .ToArray();
    }
    
    public static void Initialize()
    {
        if (!Directory.Exists(MainDirPath))
        {
            Directory.CreateDirectory(MainDirPath);
        }

        ScriptCatalog.Initialize();
    }

    public static ScriptCatalog.RefreshSummary RefreshAll(bool force = false) => ScriptCatalog.RefreshAll(force);

    public static ScriptCatalog.RequestedRefreshResult RefreshRequested(ScriptName name) =>
        ScriptCatalog.RefreshRequested(name);

    public static void Shutdown() => ScriptCatalog.Shutdown();

    public static TryGet<ScriptSection[]> GetScriptSections(string path)
    {
        try
        {
            return ScriptSection.Split(Path.GetFileNameWithoutExtension(path), File.ReadAllText(path), path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return $"Failed to read script '{path}': {exception.Message}";
        }
    }

    public static TryGet<ScriptSection> GetScriptSection(ScriptName scriptName)
    {
        return ScriptCatalog.GetSection(scriptName);
    }
    
    public static TryGet<string> GetScriptPath(ScriptName scriptName)
    {
        return ScriptCatalog.GetPath(scriptName);
    }
    
    public static bool DoesScriptExistByName(string scriptName, out string path)
    {
        if (GetScriptSection(ScriptName.CreateUnsafe(scriptName)).HasErrored(out _, out var section))
        {
            path = "";
            return false;
        }

        path = section.Path ?? "";
        return true;
    }
    
    public static bool DoesScriptExistByPath(string path)
    {
        UpdateScriptPathCollection();
        
        return RegisteredScriptPaths.Any(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsScriptOrFileName(Script script, string name)
    {
        return string.Equals(script.Name.ToString(), name, StringComparison.CurrentCultureIgnoreCase)
               || string.Equals(script.FileName.ToString(), name, StringComparison.CurrentCultureIgnoreCase);
    }

    internal static void ParseSectionSelector(string requestedName, out string fileName, out int? sectionNumber)
    {
        var separator = requestedName.LastIndexOf(':');
        if (separator > 0
            && int.TryParse(requestedName[(separator + 1)..], out var parsed)
            && parsed > 0)
        {
            fileName = StripScriptExtension(Path.GetFileName(requestedName[..separator]));
            sectionNumber = parsed;
            return;
        }

        fileName = StripScriptExtension(Path.GetFileName(requestedName));
        sectionNumber = null;
    }

    private static string StripScriptExtension(string name)
    {
        return name.EndsWith(".ser", StringComparison.OrdinalIgnoreCase)
               || name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
            ? name[..^4]
            : name;
    }

    public static ExampleGenerationSummary GenerateExamples()
    {
        var examples = ExampleHandler.GetAllExamples();

        var exampleDir = Directory.CreateDirectory(Path.Combine(MainDirPath, "Example Scripts"));
        var created = 0;
        var alreadyExisted = 0;
        foreach (var kvp in examples)
        {
            var path = Path.Combine(exampleDir.FullName, $"#{kvp.Key}.ser");
            if (File.Exists(path))
            {
                alreadyExisted++;
                continue;
            }
            
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            using var sw = File.CreateText(path);
            sw.Write(kvp.Value);
            created++;
        }

        return new ExampleGenerationSummary(created, alreadyExisted, exampleDir.FullName);
    }

    public static TryGet<string> FindFileByName(string rootDirectory, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return TryGet<string>.Error("A file name cannot be empty.");
        }

        if (Path.GetFileName(fileName) != fileName)
        {
            return TryGet<string>.Error("Audio.Load expects a file name, not a path.");
        }

        try
        {
            var root = Path.GetFullPath(rootDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!Directory.Exists(root))
            {
                return TryGet<string>.Error($"SER data directory '{root}' does not exist.");
            }

            var matches = new List<string>();
            var directories = new Stack<string>();
            directories.Push(root);

            while (directories.Count > 0)
            {
                var directory = directories.Pop();
                foreach (var file in Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly))
                {
                    if (!string.Equals(Path.GetFileName(file), fileName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) == 0)
                        matches.Add(file);
                }

                foreach (var childDirectory in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                {
                    if ((File.GetAttributes(childDirectory) & FileAttributes.ReparsePoint) == 0)
                        directories.Push(childDirectory);
                }
            }

            if (matches.Count == 0)
                return TryGet<string>.Error($"Audio file '{fileName}' was not found under the SER data directory.");

            if (matches.Count > 1)
            {
                return TryGet<string>.Error(
                    $"Multiple audio files named '{fileName}' were found. Audio file names must be unique:\n" +
                    string.Join("\n", matches.OrderBy(path => path, StringComparer.OrdinalIgnoreCase)));
            }

            return matches[0].AsSuccess();
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException
                                       or IOException or UnauthorizedAccessException
                                       or System.Security.SecurityException)
        {
            return TryGet<string>.Error($"Could not search for audio file '{fileName}': {ex.Message}");
        }
    }
}
