using System.Reflection;
using LabApi.Features.Console;
using LabApi.Loader.Features.Paths;
using SER.Examples;
using SER.FlagSystem;
using SER.Helpers.Extensions;
using SER.ScriptSystem;
using SER.ScriptSystem.Structures;

namespace SER.FileSystem;

public static class FileSystem
{
    public static readonly string MainDirPath = Path.Combine(PathManager.Configs.FullName, "Scripted Events Reloaded");
    public static readonly string DbDirPath = Path.Combine(MainDirPath, "Databases");
    public static string[] RegisteredScriptPaths = [];

    public static void UpdateScriptPathCollection()
    {
        RegisteredScriptPaths = Directory
            .GetFiles(MainDirPath, "*.txt", SearchOption.AllDirectories)
            // ignore files with a pound sign at the start
            .Where(path => Path.GetFileName(path).FirstOrDefault() != '#')
            .ToArray();
        
        //Log.Signal(RegisteredScriptPaths.JoinStrings(" "));
        
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
        ScriptFlagHandler.Clear();
        
        foreach (var scriptPath in RegisteredScriptPaths)
        {
            var scriptName = ScriptName.InitUnchecked(Path.GetFileNameWithoutExtension(scriptPath));

            var lines = Script.CreateByVerifiedPath(scriptPath, ServerConsoleExecutor.Instance).GetFlagLines();
            if (lines.IsEmpty())
            {
                continue;
            }

            ScriptFlagHandler.RegisterScript(lines, scriptName);
        }

    }
    
    public static string GetScriptPath(ScriptName scriptName)
    {
        UpdateScriptPathCollection();
        return RegisteredScriptPaths.First(p => Path.GetFileNameWithoutExtension(p) == scriptName.Value);
    }
    
    public static bool DoesScriptExist(string scriptName, out string path)
    {
        UpdateScriptPathCollection();
        
        path = RegisteredScriptPaths.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == scriptName) ?? "";
        return !string.IsNullOrEmpty(path);
    }
    
    public static bool DoesScriptExist(string path)
    {
        UpdateScriptPathCollection();
        
        return RegisteredScriptPaths.Any(p => p == path);
    }

    public static void GenerateExamples()
    {
        var examples = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(IExample).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(t => t.CreateInstance<IExample>());

        var exampleDir = Directory.CreateDirectory(Path.Combine(MainDirPath, "Example Scripts"));
        foreach (var example in examples)
        {
            var path = Path.Combine(exampleDir.FullName, $"{example.Name}.txt");
            using var sw = File.CreateText(path);
            sw.Write(example.Content);
        }
    }
}