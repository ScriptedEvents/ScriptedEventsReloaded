namespace SER.Code.Helpers;

public struct Safe<T>
{
    private readonly bool _set;

    public T Value
    {
        get
        {
            if (!_set)
                throw new InvalidOperationException($"Attempted to get {typeof(T).Name} before it was set.");
            return field;
        }
        private init
        {
            field = value;
            _set = true;
        }
    }

    public static implicit operator T(Safe<T> wrapper) => wrapper.Value;
    public static implicit operator Safe<T>(T value) => new() { Value = value };
    
    public override string ToString() => Value?.ToString() ?? "null";
}