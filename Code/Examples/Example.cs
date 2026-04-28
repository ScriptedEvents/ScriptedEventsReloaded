using System.Reflection;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;

namespace SER.Code.Examples;

public abstract class Example
{
    public abstract string Name { get; }
    public abstract string Content { get; }

    private static Dictionary<string, string>? _cachedExamples;

    [UsedImplicitly]
    public static Dictionary<string, string> GetAllExamples()
    {
        if (_cachedExamples != null) return _cachedExamples;
        
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".ser"));

        var examples = new Dictionary<string, string>();
        foreach (var name in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream == null) continue;
            using var reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            string[] parts = name.Split('.');
            if (parts.Length < 2) continue;
            string fileName = parts[parts.Length - 2];
            examples[fileName] = content;
        }

        return _cachedExamples = examples;
    }

    public static string? GetExample(string name)
    {
        return GetAllExamples().TryGetValue(name, out var content) ? content : null;
    }

    [UsedImplicitly]
    public static string Verify()
    {
        var examples = GetAllExamples();

        foreach (var example in examples)
        {
            if (Script.CreateAnonymous(example.Key, example.Value).Compile().HasErrored(out var error))
            {
                return new Result(false, $"in example '{example.Key}'") + error;
            }
        }

        return string.Empty;
    }
}