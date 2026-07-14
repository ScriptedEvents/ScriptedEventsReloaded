using LabApi.Features.Console;
using LabApi.Loader.Features.Paths;
using SER.Code.Extensions;
using SER.Code.FlagSystem;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;

namespace SER.Code.FileSystem;

public static class FileSystem
{
    public static readonly string MainDirPath = Path.Combine(PathManager.Configs.FullName, "Scripted Events Reloaded");
    public static readonly string DbDirPath = Path.Combine(MainDirPath, "Databases");
    public static readonly string ConfigsDirPath = Path.Combine(MainDirPath, "Custom Configs");
    public static string[] RegisteredScriptPaths = [];

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

            if (!path.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
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

    public static void UpdateScriptPathCollection()
    {
        List<string> paths = [];
        paths.AddRange(Directory.GetFiles(MainDirPath, "*.txt", SearchOption.AllDirectories));
        paths.AddRange(Directory.GetFiles(MainDirPath, "*.ser", SearchOption.AllDirectories));
        
        RegisteredScriptPaths = paths
            // ignore files with a pound sign at the start
            .Where(path => Path.GetFileName(path).FirstOrDefault() != '#')
            .ToArray();
        
        
        var duplicates = RegisteredScriptPaths
            .Select(Path.GetFileNameWithoutExtension)
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => (g.Key, g.Count()))
            .ToList();
        
        if (!duplicates.Any()) return;
        Logger.Error(
            $"There are {string.Join(", ", duplicates.Select(d => $"{d.Item2} scripts named '{d.Key}'"))}\n" +
            $"Please rename them to avoid conflicts."
        );
        
        RegisteredScriptPaths = RegisteredScriptPaths
            .Where(path => !duplicates.Select(d => d.Key).Contains(Path.GetFileNameWithoutExtension(path)))
            .ToArray();
    }
    
    public static void Initialize()
    {
        if (!Directory.Exists(MainDirPath))
        {
            Directory.CreateDirectory(MainDirPath);
            return;
        }

        UpdateScriptPathCollection();
        
        foreach (var scriptPath in RegisteredScriptPaths)
        {
            var fileName = Path.GetFileNameWithoutExtension(scriptPath);
            if (GetScriptSections(scriptPath).HasErrored(out var splitError, out var sections))
            {
                Log.CompileError(fileName, splitError);
                continue;
            }

            foreach (var section in sections)
            {
                var script = Script.CreateByVerifiedSection(section, ServerConsoleExecutor.Instance);
                if (script.GetFlagLines().HasErrored(out var error, out var lines))
                {
                    Log.CompileError(section.Name, error);
                    continue;
                }

                if (lines.IsEmpty())
                {
                    continue;
                }

                if (ScriptFlagHandler.RegisterScript(lines, section.Name).HasErrored(out error))
                {
                    Log.CompileError(section.Name, error);
                }
            }
        }
    }

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
        UpdateScriptPathCollection();

        var requestedName = scriptName.ToString();
        ParseSectionSelector(requestedName, out var fileName, out var requestedSection);
        var path = RegisteredScriptPaths.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == fileName);
        if (path is null)
        {
            return $"Script '{scriptName}' does not exist anymore";
        }

        if (GetScriptSections(path).HasErrored(out var error, out var sections))
        {
            return error;
        }

        if (requestedSection is { } sectionNumber)
        {
            if (sections.Length <= 1)
            {
                return $"Script '{fileName}' is not split into multiple sections.";
            }

            var section = sections.FirstOrDefault(section => section.Number == sectionNumber);
            return section is not null
                ? section
                : $"Script '{fileName}' does not contain section {sectionNumber}.";
        }

        if (sections.Length > 1)
        {
            return $"Script '{fileName}' contains {sections.Length} sections. " +
                   $"Select one using '{fileName}:1' through '{fileName}:{sections.Length}'.";
        }

        return sections[0];
    }
    
    public static TryGet<string> GetScriptPath(ScriptName scriptName)
    {
        UpdateScriptPathCollection();
        ParseSectionSelector(scriptName, out var fileName, out _);
        if (RegisteredScriptPaths.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == fileName) is
            { } path)
        {
            return path.AsSuccess();
        }

        return $"Script '{scriptName}' does not exist anymore".AsError();
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
        
        return RegisteredScriptPaths.Any(p => p == path);
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

    public static void GenerateExamples()
    {
        var examples = ExampleHandler.GetAllExamples();

        var exampleDir = Directory.CreateDirectory(Path.Combine(MainDirPath, "Example Scripts"));
        foreach (var kvp in examples)
        {
            var path = Path.Combine(exampleDir.FullName, $"{kvp.Key}.ser");
            if (File.Exists(path)) continue;
            
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            using var sw = File.CreateText(path);
            sw.Write(kvp.Value);
        }
    }
}
