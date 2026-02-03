using System.Text;

namespace SER.Code.Extensions;

public static class TypeExtensions
{
    extension(Type type)
    {
        public string AccurateName => GetAccurateName(type);
    }
    
    // done by chatgpt
    public static string GetAccurateName(this Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var sb = new StringBuilder();
        string name = type.Name;
        int index = name.IndexOf('`');
        if (index > 0)
            name = name[..index];

        sb.Append(name);
        sb.Append('<');
        var args = type.GetGenericArguments();
        for (int i = 0; i < args.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(args[i].GetAccurateName()); // recursion for nested generics
        }
        sb.Append('>');
        return sb.ToString();
    }
    
    public static object CreateInstance(this Type type)
    {
        return Activator.CreateInstance(type);
    }
    
    public static T CreateInstance<T>(this Type type)
    {
        return (T) Activator.CreateInstance(type);
    }
    
    public static string FriendlyTypeName(this Type type, bool lowerCase = false)
    {
        return type.Name.Spaceify(lowerCase);
    }
}