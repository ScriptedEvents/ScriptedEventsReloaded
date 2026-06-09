using System.Reflection;
using LabApi.Features.Console;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem.PropertySystem;

namespace SER.Code.MethodSystem;

public static class MethodIndex
{
    public static readonly Dictionary<string, Method> NameToMethodIndex = [];
    public static readonly Dictionary<FrameworkBridge.Type, List<Method>> FrameworkDependentMethods = [];
    
    /// <summary>
    /// Initializes the method index.
    /// </summary>
    internal static void Initialize()
    {
        NameToMethodIndex.Clear();
        FrameworkDependentMethods.Clear();
        AddAllDefinedMethodsInAssembly();
    }
    
    /// <summary>
    /// Retrieves all registered methods stored within the method index.
    /// </summary>
    /// <returns>An array of Method instances representing the registered methods.</returns>
    public static Method[] GetMethods()
    {
        return NameToMethodIndex.Values.ToArray();
    }

    /// <summary>
    /// Registers all defined methods within the specified assembly or, if none is provided, in the calling assembly.
    /// This allows dynamic discovery and addition of new methods to the method index.
    /// </summary>
    /// <param name="assembly">The assembly to inspect for method definitions. If null, the calling assembly is used.</param>
    public static void AddAllDefinedMethodsInAssembly(Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        var definedMethods = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(Method).IsAssignableFrom(t))
            .Select(t =>
            {
                Log.Debug($"trying to activate {t.AccurateName}");
                return t.CreateInstance<Method>();
            })
            .Where(t =>
            {
                if (t is not IDependOnFramework framework)
                    return true;
                
                FrameworkDependentMethods.AddOrInitListWithKey(framework.DependsOn, t);
                return false;
            })
            .ToList();
        
        foreach (var method in definedMethods.OfType<Method>())
        {
            AddMethod(method);
        }
        
        Logger.Info($"'{assembly.GetName().Name}' plugin has added {definedMethods.Count} methods.");
    }

    /// <summary>
    /// Adds a new method to the method index for registration and retrieval.
    /// </summary>
    /// <param name="method">The method instance to be added. It must have a unique name within the index.</param>
    public static void AddMethod(Method method)
    {
        if (NameToMethodIndex.ContainsKey(method.Name))
        {
            Logger.Error($"Tried to register an already existing method '{method.Name}'!");
            return;
        }
        
        NameToMethodIndex.Add(method.Name, method);
        
        // Used to create all arguments, so initializers can do outside work
        // for example, EnumArgument adds its enum type to serhelp command
        _ = method.ExpectedArguments;
    }

    /// <summary>
    /// Attempts to retrieve a method by its name from the method index. If not found, suggests the closest matching method name.
    /// </summary>
    /// <param name="name">The name of the method to retrieve.</param>
    /// <returns>
    /// A <see cref="TryGet{TValue}"/> object containing the retrieved method if successful,
    /// or an error message with the closest match if the method does not exist.
    /// </returns>
    public static TryGet<Method> TryGetMethod(string name)
    {
        if (NameToMethodIndex.TryGetValue(name, out var method))
        {
            return method;
        }
        
        return $"There is no method with name '{name}'.";
    }

    internal static void LoadMethodsOfFramework(FrameworkBridge.Type framework)
    {
        foreach (var method in FrameworkDependentMethods.TryGetValue(framework, out var methods) ? methods : [])
        {
            AddMethod(method);
        }
    }

    internal static void Clear()
    {
        NameToMethodIndex.Clear();
    }
}