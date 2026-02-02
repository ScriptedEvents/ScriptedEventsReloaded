using System.Reflection;

namespace SER.Code.Helpers.Attributes;

/// <summary>
/// Used on methods meant to clean up data after a round is over.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ClearDataAttribute : Attribute
{
    public static void RunAttachedMethods()
    {
        var methods = Assembly.GetExecutingAssembly()
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) 
            .Where(m => m.GetCustomAttribute<ClearDataAttribute>() != null);
        
        foreach (var method in methods)
        {
            method.Invoke(null, null);
        }
    }
}