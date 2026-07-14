using SER.Code.Helpers.ResultSystem;

namespace SER.Code.ScriptSystem.Structures;

public sealed record ScriptSection(
    ScriptName Name,
    ScriptName FileName,
    string Content,
    uint StartLine,
    int Number,
    int SectionCount,
    string? Path = null)
{
    public static TryGet<ScriptSection[]> Split(string fileName, string content, string? path = null)
    {
        var lines = content.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        var starts = lines
            .Select((line, index) => (line, index))
            .Where(x => IsFlagDeclaration(x.line))
            .Select(x => x.index)
            .ToArray();

        var physicalName = ScriptName.CreateUnsafe(fileName);
        if (starts.Length == 0)
        {
            return new[]
            {
                new ScriptSection(physicalName, physicalName, content, 1, 0, 1, path)
            };
        }

        for (var index = 0; index < starts[0]; index++)
        {
            var preambleLine = lines[index].TrimStart();
            if (preambleLine.Length == 0 || preambleLine[0] == '#')
            {
                continue;
            }

            return $"Line {index + 1} contains executable content before the first flag. " +
                   "Only blank lines and comments may precede flagged script sections.";
        }

        var sections = new ScriptSection[starts.Length];
        for (var index = 0; index < starts.Length; index++)
        {
            var start = starts[index];
            var end = index + 1 < starts.Length ? starts[index + 1] : lines.Length;
            var sectionName = starts.Length == 1
                ? physicalName
                : ScriptName.CreateUnsafe($"{fileName}:{index + 1}");

            sections[index] = new ScriptSection(
                sectionName,
                physicalName,
                string.Join("\n", lines.Skip(start).Take(end - start)),
                (uint)start + 1,
                index + 1,
                starts.Length,
                path);
        }

        return sections;
    }

    private static bool IsFlagDeclaration(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("!--", StringComparison.Ordinal)
               && (trimmed.Length == 3 || char.IsWhiteSpace(trimmed[3]));
    }
}
