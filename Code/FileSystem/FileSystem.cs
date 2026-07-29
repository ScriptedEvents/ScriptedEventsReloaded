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
    public readonly record struct ExampleGenerationSummary(int Created, int AlreadyExisted, string DirectoryPath);

    public static readonly string MainDirPath = Path.Combine(PathManager.Configs.FullName, "Scripted Events Reloaded");
    public static readonly string DbDirPath = Path.Combine(MainDirPath, "Databases");
    public static readonly string ConfigsDirPath = Path.Combine(MainDirPath, "Custom Configs");
    public static string[] RegisteredScriptPaths = [];
    public static string[] DisabledScriptPaths = [];
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

            return path.AsSuccess();
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return TryGet<string>.Error($"Path '{name}' is invalid: {ex.Message}");
        }
    }

    public static void UpdateScriptPathCollection(bool logDuplicateErrors = true)
    {
        List<string> paths = [];
        paths.AddRange(Directory.GetFiles(MainDirPath, "*.txt", SearchOption.AllDirectories));
        paths.AddRange(Directory.GetFiles(MainDirPath, "*.ser", SearchOption.AllDirectories));

        DisabledScriptPaths = paths
            .Where(path => Path.GetFileName(path).StartsWith("#", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        RegisteredScriptPaths = paths
            // ignore files with a pound sign at the start
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
}
