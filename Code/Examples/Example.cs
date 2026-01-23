using System.Reflection;
using JetBrains.Annotations;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;

namespace SER.Code.Examples;

public abstract class Example
{
    public abstract string Name { get; }
    public abstract string Content { get; }

    [UsedImplicitly]
    public static string Verify()
    {
        var examples = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && typeof(Example).IsAssignableFrom(t))
            .Select(Activator.CreateInstance)
            .Cast<Example>()
            .ToArray();

        foreach (var example in examples)
        {
            if (Script.CreateAnonymous(example.Name, example.Content).Compile().HasErrored(out var error))
            {
                return new Result(false, $"in example '{example.Name}'") + error;
            }
        }

        return string.Empty;
    }
}