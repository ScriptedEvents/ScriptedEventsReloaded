using System.Reflection;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;

namespace SER.Code.Helpers;

public static class ExampleHandler
{
    private static Dictionary<string, string>? _cachedExamples;
    private const string RootFolder = "Example_Scripts";

    public static Dictionary<string, string> GetAllExamples()
    {
        if (_cachedExamples != null) return _cachedExamples;

        var assembly = Assembly.GetExecutingAssembly();
        // Look for resources that contain our root folder and end with .ser
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => n.Contains(RootFolder) && n.EndsWith(".ser"));

        var examples = new Dictionary<string, string>();
        foreach (var name in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream == null) continue;
            
            using var reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            
            string relativePath = GetRelativePath(name);
            examples[relativePath] = content;
        }

        return _cachedExamples = examples;
    }

    private static string GetRelativePath(string resourceName)
    {
        // 1. Find where the "Example Scripts" folder starts in the namespace string
        int rootIndex = resourceName.IndexOf(RootFolder, StringComparison.InvariantCulture);
        if (rootIndex == -1) return resourceName;

        // 2. Get everything after "Example Scripts."
        string pathWithExtension = resourceName[(rootIndex + RootFolder.Length + 1)..];

        // 3. Remove the .ser extension
        int lastDot = pathWithExtension.LastIndexOf(".ser", StringComparison.InvariantCulture);
        string pathWithoutExtension = pathWithExtension[..lastDot];

        // 4. Convert dots to directory separators (optional, but cleaner for "folders")
        // e.g., "UI.Inventory" instead of "UI/Inventory" if you prefer keeping it as a key
        return pathWithoutExtension.Replace('.', '/');
    }

    public static string? GetExample(string path)
    {
        return GetAllExamples().TryGetValue(path, out var content) ? content : null;
    }

    [UsedImplicitly]
    public static (string?, string[]) Verify()
    {
        var examples = GetAllExamples();

        foreach (var example in examples)
        {
            // example.Key now contains the folder path (e.g., "Combat/Fireball")
            if (Script.CreateAnonymous(example.Key, example.Value).Compile().HasErrored(out var error))
            {
                return (new Result(false, $"in example '{example.Key}'") + error.AsError(), examples.Keys.ToArray());
            }
        }

        var regressionScripts = new Dictionary<string, string>
        {
            ["event_toggle_returns"] =
                "$disabled = DisableEvent \"Hurting\"\n$enabled = EnableEvent \"Hurting\"",
            ["sermethod_returning_shape"] = "DB.Exists \"..\""
        };

        foreach (var regressionScript in regressionScripts)
        {
            var script = Script.CreateAnonymous(regressionScript.Key, regressionScript.Value);
            if (script.Compile().HasErrored(out var error))
            {
                return (new Result(false, $"in regression script '{regressionScript.Key}'") + error.AsError(),
                    examples.Keys.ToArray());
            }

            if (regressionScript.Key == "sermethod_returning_shape" &&
                !script.IsSingleSynchronousReturningMethod)
            {
                return ("The sermethod regression was not recognized as a synchronous returning method.",
                    examples.Keys.ToArray());
            }
        }

        var invalidScripts = new Dictionary<string, string>
        {
            ["missing_end"] = "if true\n    Print \"this block is intentionally not closed\"",
            ["extra_end"] = "end"
        };

        foreach (var invalidScript in invalidScripts)
        {
            if (!Script.CreateAnonymous(invalidScript.Key, invalidScript.Value)
                    .Compile()
                    .HasErrored())
            {
                return ($"The validator accepted invalid regression script '{invalidScript.Key}'.",
                    examples.Keys.ToArray());
            }
        }

        return (null, examples.Keys.ToArray());
    }
}
