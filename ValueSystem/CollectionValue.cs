using System.Collections;
using SER.Helpers.Exceptions;
using SER.Helpers.ResultSystem;

namespace SER.ValueSystem;

public class CollectionValue(IEnumerable value) : Value
{
    public Value[] CastedValues
    {
        get
        {
            if (field is not null) return field;
            
            List<Value> list = [];
            list.AddRange(from object item in value select Parse(item));
            
            if (list.Select(i => i.GetType()).Distinct().Count() > 1)
            {
                throw new ScriptRuntimeError("Collection was detected with mixed types.");
            }
            
            return field = list.ToArray();
        }
    } = null!;
    
    public TryGet<Value> GetAt(int index)
    {
        if (index < 1) return $"Provided index {index}, but index cannot be less than 1";
        
        try
        {
            return CastedValues[index - 1];
        }
        catch (IndexOutOfRangeException)
        {
            return $"There is no value at index {index}";
        }
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", CastedValues.Select(v => v.ToString()))}]";
    }
}