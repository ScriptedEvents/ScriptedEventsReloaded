using System.Reflection;

namespace SER.Code.Helpers;

public static class ReflectionHelper
{
    public static bool ShouldBeConsidered(Assembly assembly)
    {
        var name = assembly.GetName().Name;
        return name.StartsWith("UnityEngine") 
               || name.StartsWith("LabApi") 
               || name.StartsWith("NorthwoodLib")
               || name.StartsWith("PluginAPI") 
               || name.StartsWith("Mirror") 
               || name.StartsWith("SER")
               || name.StartsWith("Assembly-CSharp");
    }
}