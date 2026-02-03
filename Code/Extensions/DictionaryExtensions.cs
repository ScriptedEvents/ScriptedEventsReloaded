namespace SER.Code.Extensions;

public static class DictionaryExtensions
{
    public static void AddOrInitListWithKey<TKey, TCollection, TCollectionValue>(
        this Dictionary<TKey, TCollection> dictionary, 
        TKey key, 
        TCollectionValue value
    ) where TCollection : List<TCollectionValue>, new()
    {
        if (dictionary.TryGetValue(key, out var list))
        {
            list.Add(value);
        }
        else
        {
            dictionary[key] = [value];
        }
    }
    
    public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
    {
        key = tuple.Key;
        value = tuple.Value;
    }
}