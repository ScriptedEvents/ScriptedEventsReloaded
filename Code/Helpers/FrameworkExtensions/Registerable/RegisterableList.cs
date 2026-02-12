#nullable enable
namespace SER.Code.Helpers;

public class RegisterableList<T> : List<T>
{
    public Action<T> OnAdd { get; set; }
    public Action<T> OnRemove { get; set; }
    public Action OnClear { get; set; }

    public RegisterableList()
    {
        
    }
    
    public RegisterableList(Action<T> onAdd, Action<T> onRemove, Action onClear)
    {
        OnAdd = onAdd;
        OnRemove = onRemove;
        OnClear = onClear;
    }
    
    public new void Add(T item)
    {
        OnAdd?.Invoke(item);
        base.Add(item);
    }
    
    public new bool Remove(T item)
    {
        OnRemove?.Invoke(item);
        return base.Remove(item);
    }

    public new void Clear()
    {
        OnClear?.Invoke();
        foreach (var i in this)
        {
            OnRemove?.Invoke(i);
        }
        base.Clear();
    }

    public new void RemoveAll(Predicate<T> match)
    {
        foreach (var item in this)
        {
            if(match(item)) OnRemove?.Invoke(item);
        }
        base.RemoveAll(match);
    }
}