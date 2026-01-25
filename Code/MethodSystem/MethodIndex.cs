using System.Reflection;
using LabApi.Features.Console;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem;

public static class MethodIndex
{
    private static readonly Dictionary<string, Method> NameToMethodIndex = [];
    private static readonly HashSet<Type> ExiledMethods = [];
    
    /// <summary>
    /// Initializes the method index.
    /// </summary>
    internal static void Initialize()
    {
        NameToMethodIndex.Clear();
        
        AddAllDefinedMethodsInAssembly();
        
        ExiledHelper.OnExiledDetected += () =>
        {
            foreach (var method in ExiledMethods)
            {
                AddMethod((Method)Activator.CreateInstance(method));
            }
        };
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
            .Where(t =>
            {
                if (!typeof(IExiledMethod).IsAssignableFrom(t)) return true;
                
                ExiledMethods.Add(t);
                return false;
            })
            .Select(t => Activator.CreateInstance(t) as Method)
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
        
        var closestMethod = NameToMethodIndex.Keys
            .OrderBy(x => LevenshteinDistance(x, name))
            .FirstOrDefault();
        
        return $"There is no method with name '{name}'. Did you mean '{closestMethod ?? "<error>"}'?";
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings, which represents the minimum number of
    /// single-character edits (insertions, deletions, or substitutions) required to transform one string to another.
    /// </summary>
    /// <param name="a">The first string to compare.</param>
    /// <param name="b">The second string to compare.</param>
    /// <returns>The Levenshtein distance between the two strings.</returns>
    private static int LevenshteinDistance(string a, string b)
    {
        int[,] dp = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
            dp[i, 0] = i;

        for (int j = 0; j <= b.Length; j++)
            dp[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;

                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, 
                        dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost    
                );
            }
        }

        return dp[a.Length, b.Length];
    }


    internal static void Clear()
    {
        NameToMethodIndex.Clear();
    }
}