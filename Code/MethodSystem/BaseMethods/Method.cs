using MEC;
using SER.Code.ArgumentSystem;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ContextSystem;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem;

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
            .WithCurrent(name =>
            {
                if (name.EndsWith("Methods")) return name[..^"Methods".Length];
                return name;
            })
            .Replace("_", " ")
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
    
    public Script? Script { get; set; } = null;

    public readonly string Subgroup;

    public uint? LineNum { get; set; }
    
    private readonly List<CoroutineHandle> _coroutines = [];
    
    protected CoroutineHandle RunCoroutine(IEnumerator<float> coro)
    {
        if (Script is null) throw new AnonymousUseException("Cannot run coroutine without a script.");
        
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

    public static string GetDoc<T>(params string[] arguments) where T : Method, new()
    {
        var formatted = $"{GetFriendlyName(typeof(T))} {arguments.JoinStrings(" ")}";
        
        if (Tokenizer.TokenizeLine(formatted, null, null).HasErrored(out var error, out var tokens) 
            || Contexter.ContextLine(tokens, null, null).HasErrored(out error))
        {
            Log.Debug(error);
            throw new Exception($"Method '{formatted}' used in documentation has invalid syntax.");
        }

        return formatted;
    }
}