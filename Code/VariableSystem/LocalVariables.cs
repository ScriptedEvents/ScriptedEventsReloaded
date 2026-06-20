using SER.Code.Extensions;
using SER.Code.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;

namespace SER.Code.VariableSystem;

public class LocalVariables(Script script)
{
    private Value[] _storage = new Value[16];
    private int _count = 0;
    
    private readonly Dictionary<VariableRepr, int> _nameToIndex = new();
    
    public int GetOrCreateIndex(char prefix, string name)
    {
        if (_nameToIndex.TryGetValue(new(prefix, name), out int index))
            return index;

        index = _count++;
        if (index >= _storage.Length)
            Array.Resize(ref _storage, _storage.Length * 2);

        _nameToIndex[new(prefix, name)] = index;
        return index;
    }
    
    public void Set(int index, Value value) => _storage[index] = value;
    
    public void Set(string name, Value value) => Set(GetOrCreateIndex(value.Prefix, name), value);
    
    public TryGet<Value> Get(int index) => _storage.TryGet(index, out var value)
        ? value
        : $"Variable under index {index} does not exist.".AsError();
}