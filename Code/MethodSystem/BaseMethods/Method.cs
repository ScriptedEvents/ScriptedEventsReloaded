using MEC;
using SER.Code.ArgumentSystem;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.ScriptSystem;

namespace SER.Code.MethodSystem.BaseMethods;

/// <summary>
///     Represents a base method.
/// </summary>
/// <remarks>
///     Do NOT use this to define a SER method, as it has no Execute() method.
///     Use <see cref="SynchronousMethod" /> or <see cref="YieldingMethod" />.
/// </remarks>
public abstract class Method
{
    private static readonly Dictionary<Type, (string Name, string Subgroup)> MethodMetadataCache = new();

    protected Method()
    {
        var type = GetType();

        if (MethodMetadataCache.TryGetValue(type, out var cached))
        {
            Name = cached.Name;
            Subgroup = cached.Subgroup;
        }
        else
        {
            Subgroup = type.Namespace?
                .Split('.')
                .LastOrDefault()?
                .WithCurrent(name =>
                {
                    if (name.EndsWith("Methods")) return name[..^"Methods".Length];
                    return name;
                })
                .Replace("_", " ")
                       ?? "Unknown";

            var name = type.Name.Replace("_", ".");
            if (!name.EndsWith("Method"))
            {
                throw new CoreInvariantException($"Method class name '{name}' must end with 'Method'.");
            }

            Name = name[..^"Method".Length];
            MethodMetadataCache[type] = (Name, Subgroup);
        }

        Args = new(this);
    }

    public readonly string Name;
    
    public abstract string Description { get; }
    
    public abstract Argument[] ExpectedArguments { get; }
    
    public ProvidedArguments Args { get; }
    
    public Script Script { get; set; } = null!;

    public readonly string Subgroup;

    public uint? LineNum { get; set; }
    
    protected CoroutineHandle RunCoroutine(IEnumerator<float> coro)
    {
        return coro.Run(Script);
    }

    public override string ToString()
    {
        return LineNum.HasValue
            ? $"{Name} method in line {LineNum}"
            : $"{Name} method";
    }
    
    public static string NameOfMethod(Type type) => type.Name[..^"Method".Length].Replace("_", ".");
}
