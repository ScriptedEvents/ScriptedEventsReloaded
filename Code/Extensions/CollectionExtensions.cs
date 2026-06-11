using SER.Code.Helpers.ResultSystem;

namespace SER.Code.Extensions;

public static class CollectionExtensions
{
    extension<T>(IEnumerable<T> enumerable)
    {
        public void ForEachItem(Action<T> obj)
        {
            var list = enumerable as List<T> ?? enumerable.ToList();

            foreach (var value in list)
            {
                obj?.Invoke(value);
            }
        }
        
        public T GetRandomValue()
        {
            var array = enumerable as T[] ?? enumerable.ToArray();
            return array[new Random().Next(0, array.Length)];
        }

        public TryGet<T> TryGetRandomValue(string error)
        {
            try
            {
                return enumerable.GetRandomValue();
            }
            catch
            {
                return error;
            }
        }
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
    
    extension<T>(IEnumerable<T> enumerable)
    {
        public int GetEnumerableHashCode()
        {
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
        public IEnumerable<T> Without(T value)
        {
            return enumerable.Where(x => x?.Equals(value) is false);
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