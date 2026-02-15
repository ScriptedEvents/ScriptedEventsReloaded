using System.Reflection;
using JetBrains.Annotations;
using SER.Code.ContextSystem.Interfaces;
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

        var keywords = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && typeof(IKeywordContext).IsAssignableFrom(t))
            .Select(Activator.CreateInstance)
            .Cast<IKeywordContext>()
            .Where(k => k.Example != null)
            .ToArray();
        
        foreach (var keyword in keywords)
        {
            if (Script.CreateAnonymous(keyword.KeywordName, keyword.Example!).Compile().HasErrored(out var error))
            {
                return new Result(false, $"in example of keyword '{keyword.KeywordName}'") + error;
            }
        }

        return string.Empty;
    }
}