using MEC;
using SER.Code.ArgumentSystem;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
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
    protected Method()
    {
        var type = GetType();
        
        Subgroup = type.Namespace?
            .Split('.')
            .LastOrDefault()?
            .WithCurrent(name => name[..^"Methods".Length]) 
                   ?? "Unknown";
        
        var name = type.Name;
        if (!name.EndsWith("Method"))
        {
            throw new AndrzejFuckedUpException($"Method class name '{name}' must end with 'Method'.");
        }
        
        Name = name[..^"Method".Length];
        Args = new(this);
    }

    public readonly string Name;
    
    public abstract string Description { get; }
    
    public abstract Argument[] ExpectedArguments { get; }
    
    public ProvidedArguments Args { get; }
    
    public Script Script { get; set; } = null!;

    public readonly string Subgroup;

    public uint? LineNum { get; set; }
    
    private readonly List<CoroutineHandle> _coroutines = [];
    
    protected CoroutineHandle RunCoroutine(IEnumerator<float> coro)
    {
        var handle = coro.Run(Script);
        _coroutines.Add(handle);
        return handle;
    }

    public override string ToString()
    {
        return LineNum.HasValue
            ? $"{Name} method in line {LineNum}"
            : $"{Name} method";
    }
    
    public static string GetFriendlyName(Type type) => type.Name[..^"Method".Length];
}