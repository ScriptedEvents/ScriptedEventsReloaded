using System.Reflection;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;

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
            if (ScriptSection.Split(example.Key, example.Value)
                .HasErrored(out var exampleSplitError, out var exampleSections))
            {
                return (new Result(false, $"while splitting example '{example.Key}'") + exampleSplitError.AsError(),
                    examples.Keys.ToArray());
            }

            foreach (var section in exampleSections)
            {
                if (Script.CreateByVerifiedSection(section, ScriptExecutor.Get())
                    .Compile()
                    .HasErrored(out var error))
                {
                    return (new Result(false, $"in example section '{section.Name}'") + error.AsError(),
                        examples.Keys.ToArray());
                }
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

        const string multiSectionContent =
            "# file comment\n\n" +
            "!-- OnEvent RoundStarted\n" +
            "Print \"round started\"\n\n" +
            "!-- CustomCommand status\n" +
            "Print \"status\"";

        if (ScriptSection.Split("multi_section", multiSectionContent)
                .HasErrored(out var splitError, out var sections))
        {
            return ($"The multi-section regression could not be split: {splitError}", examples.Keys.ToArray());
        }

        if (sections.Length != 2
            || sections[0].Name.ToString() != "multi_section:1"
            || sections[0].StartLine != 3
            || sections[1].Name.ToString() != "multi_section:2"
            || sections[1].StartLine != 6
            || sections[0].Content.Contains("CustomCommand"))
        {
            return ("The multi-section regression produced incorrect boundaries or identities.",
                examples.Keys.ToArray());
        }

        foreach (var section in sections)
        {
            if (Script.CreateByVerifiedSection(section, ScriptExecutor.Get())
                    .Compile()
                    .HasErrored(out var sectionError))
            {
                return ($"Multi-section regression '{section.Name}' failed to compile: {sectionError}",
                    examples.Keys.ToArray());
            }
        }

        if (!ScriptSection.Split("invalid_preamble", "Print \"outside\"\n!-- Function\nPrint \"inside\"")
                .HasErrored())
        {
            return ("The multi-section splitter accepted executable content before the first flag.",
                examples.Keys.ToArray());
        }

        FileSystem.FileSystem.ParseSectionSelector("cRoleSpawn:3", out var selectedFile, out var selectedSection);
        if (selectedFile != "cRoleSpawn" || selectedSection != 3)
        {
            return ("A bare multi-section selector did not resolve to its physical script file.",
                examples.Keys.ToArray());
        }

        FileSystem.FileSystem.ParseSectionSelector(
            @"C:\SER\custom roles\cRoleSpawn.ser:1",
            out selectedFile,
            out selectedSection);
        if (selectedFile != "cRoleSpawn" || selectedSection != 1)
        {
            return ("A full-path multi-section selector did not resolve to its physical script file.",
                examples.Keys.ToArray());
        }

        if (!CRole.PassesSpawnChance(1f, 0.999999999)
            || CRole.PassesSpawnChance(0f, 0d)
            || !CRole.PassesSpawnChance(0.5f, 0.49d)
            || CRole.PassesSpawnChance(0.5f, 0.5d)
            || CRole.GetCappedSpawnCount(5, 1) != 1
            || CRole.GetCappedSpawnCount(5, null) != 5
            || CRole.GetCappedSpawnCount(5, -1) != 0)
        {
            return ("Custom-role spawn chance boundary handling is incorrect.", examples.Keys.ToArray());
        }

        const string invalidSecondSection =
            "!-- Function\n" +
            "Print \"valid\"\n" +
            "!-- Function\n" +
            "end";
        if (ScriptSection.Split("section_lines", invalidSecondSection)
                .HasErrored(out splitError, out sections)
            || !Script.CreateByVerifiedSection(sections[1], ScriptExecutor.Get())
                .Compile()
                .HasErrored(out var lineError)
            || !lineError.Contains("Line 4"))
        {
            return ("Multi-section compilation did not preserve physical source line numbers.",
                examples.Keys.ToArray());
        }

        return (null, examples.Keys.ToArray());
    }
}
