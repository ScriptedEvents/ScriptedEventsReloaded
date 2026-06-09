using LabApi.Features.Console;
using LabApi.Loader.Features.Paths;
using SER.Code.Extensions;
using SER.Code.FlagSystem;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.FileSystem;

public static class FileSystem
{
    public static readonly string MainDirPath = Path.Combine(PathManager.Configs.FullName, "Scripted Events Reloaded");
    public static readonly string DbDirPath = Path.Combine(MainDirPath, "Databases");
    public static readonly string ConfigsDirPath = Path.Combine(MainDirPath, "Custom Configs");
    public static string[] RegisteredScriptPaths = [];

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
            var scriptName = ScriptName.CreateUnsafe(Path.GetFileNameWithoutExtension(scriptPath));

            if (Script
                .CreateByVerifiedPath(scriptPath, ServerConsoleExecutor.Instance)
                .GetFlagLines()
                .HasErrored(out var error, out var lines))
            {
                Log.CompileError(scriptName, error);
                continue;
            }
            
            if (lines.IsEmpty())
            {
                continue;
            }

            ScriptFlagHandler.RegisterScript(lines, scriptName);
        }
    }
    
    public static TryGet<string> GetScriptPath(ScriptName scriptName)
    {
        UpdateScriptPathCollection();
        if (RegisteredScriptPaths.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == scriptName) is
            { } path)
        {
            return path.AsSuccess();
        }

        return $"Script '{scriptName}' does not exist anymore".AsError();
    }
    
    public static bool DoesScriptExistByName(string scriptName, out string path)
    {
        UpdateScriptPathCollection();
        
        path = RegisteredScriptPaths.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == scriptName) ?? "";
        return !string.IsNullOrEmpty(path);
    }
    
    public static bool DoesScriptExistByPath(string path)
    {
        UpdateScriptPathCollection();
        
        return RegisteredScriptPaths.Any(p => p == path);
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