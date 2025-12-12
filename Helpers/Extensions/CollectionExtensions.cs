using SER.Helpers.ResultSystem;
using Random = UnityEngine.Random;

namespace SER.Helpers.Extensions;

public static class CollectionExtensions
{
    public static void ForEachItem<T>(this IEnumerable<T> enumerable, Action<T> obj)
    {
        var list = enumerable as List<T> ?? enumerable.ToList();

        foreach (var value in list)
        {
            obj?.Invoke(value);
        }
    }
    
    public static T? GetRandomValue<T>(this IEnumerable<T> enumerable)
    {
        var array = enumerable.ToArray<T>();
        return array.Length != 0 ? array[Random.Range(0, array.Length)] : default (T);
    }

    public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> enumerable)
    {
        return enumerable.Where(x => x != null)!;
    }

    public static IEnumerable<TResult> Flatten<TResult>(
        this IEnumerable<IEnumerable<TResult>> source)
    {
        return source.SelectMany(x => x);
    }

    public static string JoinStrings(this IEnumerable<string> source, string separator)
    {
        return string.Join(separator, source);
    }
    public static TryGet<int> GetEnumerableHashCode<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable is null) return $"{nameof(enumerable)} is null.";

        unchecked
        {
            var hashCode = 17;
            foreach (var item in enumerable)
            {
                hashCode = hashCode * 31 + (item?.GetHashCode() ?? 0);
            }
            return hashCode;
        }
    }

    extension<T>(List<T> list)
    {
        public uint Len => (uint) list.Count;
    }
    
    extension<T>(T[] array)
    {
        public uint Len => (uint) array.Length;
    }
}